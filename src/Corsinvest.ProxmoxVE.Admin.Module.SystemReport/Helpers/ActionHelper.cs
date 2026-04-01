/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;
using Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Persistence;
using Corsinvest.ProxmoxVE.Report;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Helpers;

internal class ActionHelper : BaseActionHelper<Module, Settings, DataChangedNotification>
{
    public static async Task GenerateAsync(IServiceScope scope, int id)
    {
        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(ActionHelper));
        var auditService = scope.GetAuditService();
        var taskTracker = scope.GetRequiredService<ITaskTrackerService>();
        await using var db = await scope.GetDbContextAsync<ModuleDbContext>();
        var job = (await db.JobResults.FromIdAsync(id))!;
        job.Start = DateTime.UtcNow;

        await using var taskScope = await taskTracker.StartAsync($"System Report [{job.ClusterName}]", job.ClusterName, GetModule(scope).Name, id.ToString(), GetModule(scope).LinkMain?.GetRealUrl(job.ClusterName));
        try
        {
            using (logger.LogTimeOperation(LogLevel.Information, true, "Execute System Report for cluster '{clusterName}'", job.ClusterName))
            {
                var clusterClient = scope.GetClusterClient(job.ClusterName);
                var client = await clusterClient.GetPveClientAsync();

                var engine = new ReportEngine(client, job.Settings);

                var progress = new Progress<ReportProgress>(p =>
                {
                    job.Logs += $"{DateTime.UtcNow:O} {p}\n";
                    taskScope.Log(p.ToString());
                });

                await using var stream = await engine.GenerateAsync(progress);
                await using var file = File.Create(job.FileName);
                await stream.CopyToAsync(file);
            }

            job.End = DateTime.UtcNow;
            job.Status = true;
            await db.SaveChangesAsync();

            await auditService.LogAsync("SystemReport.Scan",
                                        true,
                                        $"Job ID: {id}, Cluster: {job.ClusterName}");

            taskScope.Log($"Job ID: {id}, Cluster: {job.ClusterName}");
            await PublishDataChangedAsync(scope);
        }
        catch (Exception ex)
        {
            taskScope.Item.Status = TaskItemStatus.Failed;
            taskScope.Log(ex.ToString(), LogLevel.Error);
            throw;
        }
    }
}
