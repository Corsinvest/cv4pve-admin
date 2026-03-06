/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;
using Microsoft.Extensions.Localization;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.TaskTracking.Service;

internal class TaskTrackerService(IDbContextFactory<ModuleDbContext> dbContextFactory,
                                  IModuleService moduleService,
                                  IServiceScopeFactory scopeFactory,
                                  ILogger<TaskTrackerService> logger,
                                  IStringLocalizer<TaskTrackerService> L) : ITaskTrackerService
{
    private readonly ConcurrentDictionary<int, TaskScope> _active = new();

    public event EventHandler<TaskItem>? Changed;
    public event EventHandler? AcknowledgedChanged;

    private void RaiseChanged(TaskItem item) => Changed?.Invoke(this, item);

    public async Task<TaskScope> StartAsync(string title,
                                            string clusterName,
                                            string? moduleName = null,
                                            string? referenceId = null,
                                            string? detailUrl = null,
                                            TimeSpan? timeoutAfter = null,
                                            bool cancellable = true)
    {
        using var diScope = scopeFactory.CreateScope();
        var userName = diScope.ServiceProvider.GetRequiredService<ICurrentUserService>().UserName;
        var createdBy = string.IsNullOrWhiteSpace(userName) || userName == "anonymous"
                            ? "system"
                            : userName;

        var item = new TaskItem
        {
            Title = title,
            ClusterName = clusterName,
            ModuleName = moduleName,
            ReferenceId = referenceId,
            DetailUrl = detailUrl,
            TimeoutAfter = timeoutAfter ?? TimeSpan.FromHours(1),
            IsCancellable = cancellable,
            CreatedBy = createdBy,
        };

        await using var db = await dbContextFactory.CreateDbContextAsync();
        db.TaskItems.Add(item);
        await db.SaveChangesAsync();

        var scope = new TaskScope(item);
        _active[item.Id] = scope;

        _ = PersistOnCompletionAsync(item.Id, scope);

        RaiseChanged(item);
        return scope;
    }

    public async Task CancelAsync(int id)
    {
        TaskItem? cancelledItem = null;
        if (_active.TryRemove(id, out var scope))
        {
            cancelledItem = scope.Item;
            scope.RequestCancel();
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        await db.TaskItems.Where(t => t.Id == id)
                          .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, TaskItemStatus.Cancelled)
                                                    .SetProperty(t => t.EndedAt, DateTime.UtcNow));

        if (cancelledItem != null) { RaiseChanged(cancelledItem); }
    }

    public async Task AcknowledgeAsync(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0) { return; }
        await using var db = await dbContextFactory.CreateDbContextAsync();
        await db.TaskItems.Where(t => idList.Contains(t.Id))
                          .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsAcknowledged, true));
        AcknowledgedChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task<int> CleanupAsync(int retentionDays)
    {
        await using var taskScope = await StartAsync(L["Task History Cleanup"],
                                                     ApplicationHelper.AllClusterName,
                                                     cancellable: false);
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
            await using var db = await dbContextFactory.CreateDbContextAsync();
            var deleted = await db.TaskItems
                                  .Where(t => t.Status != TaskItemStatus.Running && t.EndedAt < cutoff)
                                  .ExecuteDeleteAsync();
            taskScope.Log(L["Deleted {0} record(s) older than {1} days", deleted, retentionDays]);
            return deleted;
        }
        catch (Exception ex)
        {
            taskScope.Item.Status = TaskItemStatus.Failed;
            taskScope.Log(ex.ToString(), LogLevel.Error);
            throw;
        }
    }

    public async Task AbandonStaleTasksAsync()
    {
        await using var db = await dbContextFactory.CreateDbContextAsync();
        var now = DateTime.UtcNow;
        var affected = await db.TaskItems
                               .Where(t => t.Status == TaskItemStatus.Running)
                               .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, TaskItemStatus.Abandoned)
                                                         .SetProperty(t => t.EndedAt, now));

        if (affected > 0)
        {
            logger.LogInformation("Startup: marked {Count} stale running task(s) as Abandoned", affected);
        }
    }

    public async Task<IReadOnlyList<TResult>> GetRecentAsync<TResult>(ClaimsPrincipal principal,
                                                                      Expression<Func<TaskItem, TResult>> selector,
                                                                      int max = 15,
                                                                      Expression<Func<TaskItem, bool>>? filter = null)
    {
        var allowedModules = moduleService.Modules
                                          .Where(m => principal.IsInRole(m.RoleAdmin.Key), principal != null)
                                          .Select(m => m.Name)
                                          .ToList();

        await using var db = await dbContextFactory.CreateDbContextAsync();

        await PurgeZombiesAsync(db);

        var query = db.TaskItems
                      .Where(a => a.ModuleName == null || allowedModules.Contains(a.ModuleName));

        var running = await query.Where(t => t.Status == TaskItemStatus.Running)
                                 .OrderByDescending(t => t.StartedAt)
                                 .AsNoTracking()
                                 .Select(selector)
                                 .ToListAsync();

        var remaining = max - running.Count;
        var finishing = remaining > 0
            ? await query.Where(t => t.Status != TaskItemStatus.Running)
                         .Where(filter ?? (t => true))
                         .OrderByDescending(t => t.EndedAt ?? t.StartedAt)
                         .Take(remaining)
                         .AsNoTracking()
                         .Select(selector)
                         .ToListAsync()
            : [];

        return [.. running.Concat(finishing)];
    }

    public async Task<IReadOnlyList<string>> GetLogsAsync(int id)
    {
        if (_active.TryGetValue(id, out var scope))
        {
            return scope.Item.Logs.AsReadOnly();
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var logs = await db.TaskItems.Where(t => t.Id == id)
                                     .Select(t => t.Logs)
                                     .FirstOrDefaultAsync();
        return logs ?? [];
    }

    private async Task PersistOnCompletionAsync(int id, TaskScope scope)
    {
        var tcs = new TaskCompletionSource();
        Action onUpdate = () => _ = FlushLogsAsync(id, scope);
        Action onEnd = () => tcs.TrySetResult();

        scope.Item.Updated += onUpdate;
        scope.Item.TaskEnded += onEnd;

        try
        {
            await tcs.Task;

            scope.Item.Updated -= onUpdate;
            scope.Item.TaskEnded -= onEnd;

            _active.TryRemove(id, out _);

            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.TaskItems.Where(t => t.Id == id)
                              .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, scope.Item.Status)
                                                        .SetProperty(t => t.EndedAt, scope.Item.EndedAt)
                                                        .SetProperty(t => t.Logs, scope.Item.Logs)
                                                        .SetProperty(t => t.LastLog, scope.Item.LastLog)
                                                        .SetProperty(t => t.DetailUrl, scope.Item.DetailUrl)
                                                        .SetProperty(t => t.LastActivity, scope.Item.LastActivity)
                                                        .SetProperty(t => t.Progress, scope.Item.Progress));
            RaiseChanged(scope.Item);
        }
        catch (Exception ex)
        {
            scope.Item.Updated -= onUpdate;
            scope.Item.TaskEnded -= onEnd;
            logger.LogError(ex, "Failed to persist completion for task {TaskId}", id);
            _active.TryRemove(id, out _);
        }
    }

    private async Task FlushLogsAsync(int id, TaskScope scope)
    {
        try
        {
            await using var db = await dbContextFactory.CreateDbContextAsync();
            await db.TaskItems.Where(t => t.Id == id)
                              .ExecuteUpdateAsync(s => s.SetProperty(t => t.Logs, scope.Item.Logs)
                                                        .SetProperty(t => t.LastLog, scope.Item.LastLog)
                                                        .SetProperty(t => t.LastActivity, scope.Item.LastActivity)
                                                        .SetProperty(t => t.Progress, scope.Item.Progress));
            RaiseChanged(scope.Item);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to flush logs for task {TaskId}", id);
        }
    }

    private async Task PurgeZombiesAsync(ModuleDbContext db)
    {
        var now = DateTime.UtcNow;
        var zombies = await db.TaskItems
                              .Where(t => t.Status == TaskItemStatus.Running
                                       && t.LastActivity < now - t.TimeoutAfter)
                              .ToListAsync();

        foreach (var z in zombies)
        {
            z.Status = TaskItemStatus.Abandoned;
            z.EndedAt = now;
            z.AddLog(L["Task terminated: exceeded timeout of {0}", z.TimeoutAfter], LogLevel.Warning);
            _active.TryRemove(z.Id, out _);
        }

        if (zombies.Count > 0) { await db.SaveChangesAsync(); }
    }
}
