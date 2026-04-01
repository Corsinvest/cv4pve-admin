/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Net.Mime;
using System.Text.RegularExpressions;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.TaskTracking;
using Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;
using Corsinvest.ProxmoxVE.Diagnostic.Api;
using Microsoft.AspNetCore.WebUtilities;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Helpers;

internal class ActionHelper : BaseActionHelper<Module, Settings, DataChangedNotification>
{
    public static async Task ScanAsync(IServiceScope scope, string clusterName)
    {
        var logger = scope.GetLoggerFactory().CreateLogger<ActionHelper>();
        var auditService = scope.GetAuditService();
        var settings = GetModuleSettings(scope, clusterName);
        var taskTracker = scope.GetRequiredService<ITaskTrackerService>();

        await using var taskScope = await taskTracker.StartAsync($"Diagnostic scan [{clusterName}]", clusterName, GetModule(scope).Name);
        try
        {
            using (logger.LogTimeOperation(LogLevel.Information, true, "Execute diagnostic for cluster '{clusterName}'", clusterName))
            {
                await using var db = await scope.GetDbContextAsync<ModuleDbContext>();

                taskScope.Item.Phase = "Loading ignored issues";

                var ignoredIssues = db.IgnoredIssues
                                      .FromClusterName(clusterName)
                                      .Select(a => new DiagnosticResult
                                      {
                                          Context = a.Context,
                                          Description = Regex.Escape(a.Description!),
                                          Gravity = a.Gravity,
                                          SubContext = Regex.Escape(a.SubContext!),
                                          Id = Regex.Escape(a.IdResource!)
                                      })
                                      .ToList();

                var now = DateTime.UtcNow;

                taskScope.Item.Phase = "Analyzing cluster";

                var client = await scope.GetClusterClient(clusterName).GetPveClientAsync();
                var details = (await new DiagnosticEngine(client, settings.ApiSettings).AnalyzeAsync(ignoredIssues))
                                         .Select(a => new JobDetail
                                         {
                                             IdResource = a.Id,
                                             Context = a.Context,
                                             Description = a.Description,
                                             Gravity = a.Gravity,
                                             IsIgnoredIssue = a.IsIgnoredIssue,
                                             SubContext = a.SubContext
                                         })
                                         .ToList();

                int Count(DiagnosticResultGravity gravity) => details.Count(a => a.Gravity == gravity && !a.IsIgnoredIssue);

                taskScope.Item.Phase = "Saving results";

                var jobResult = new JobResult
                {
                    ClusterName = clusterName,
                    Start = now,
                    End = DateTime.UtcNow,
                    Details = details,
                    Critical = Count(DiagnosticResultGravity.Critical),
                    Info = Count(DiagnosticResultGravity.Info),
                    Warning = Count(DiagnosticResultGravity.Warning)
                };

                await db.JobResults.AddAsync(jobResult);
                await db.SaveChangesAsync();

                taskScope.Item.ReferenceId = jobResult.Id.ToString();
                taskScope.Item.DetailUrl = QueryHelpers.AddQueryString(GetModule(scope).LinkMain!.GetRealUrl(clusterName),
                                                                       nameof(Components.Scans.Id),
                                                                       jobResult.Id.ToString());

                //delete old
                var ids = await db.JobResults.FromClusterName(clusterName)
                                             .OrderByDescending(a => a.Start)
                                             .Skip(settings.Keep)
                                             .Select(a => a.Id)
                                             .ToListAsync();

                await db.JobResults.Where(a => ids.Contains(a.Id)).ExecuteDeleteAsync();

                //send notification
                taskScope.Item.Phase = "Sending notifications";
                if (settings.NotifierConfigurations?.Any() is true)
                {
                    var diagnosticService = scope.GetRequiredService<IDiagnosticService>();

                    await using var ms = diagnosticService.GeneratePdf(new JobResult
                    {
                        ClusterName = clusterName,
                        Start = now,
                        Details = details,
                        Critical = Count(DiagnosticResultGravity.Critical),
                        Info = Count(DiagnosticResultGravity.Info),
                        Warning = Count(DiagnosticResultGravity.Warning)
                    });

                    var appSettings = scope.GetSettingsService().GetAppSettings();
                    var L = scope.GetRequiredService<IStringLocalizer<ActionHelper>>();

                    await scope.GetNotifierService().SendAsync(settings.NotifierConfigurations, new()
                    {
                        Subject = L["{0} - Diagnostic result of cluster '{1}'", appSettings.AppName, clusterName],
                        Body = L["Diagnostic result of {0}", now],
                        Attachments = [new(ms, "Diagnostic.pdf", MediaTypeNames.Application.Pdf)]
                    });
                }

                var totalIssues = details.Count(a => !a.IsIgnoredIssue);
                var contextCounts = details.Where(a => !a.IsIgnoredIssue)
                                          .GroupBy(a => a.Context)
                                          .Select(g => $"{g.Key}: {g.Count()}")
                                          .JoinAsString(", ");

                await auditService.LogAsync("Diagnostic.Scan",
                                            true,
                                            $"Cluster: {clusterName}, " +
                                            $"Issues: {totalIssues} ({contextCounts}), " +
                                            $"Critical: {Count(DiagnosticResultGravity.Critical)}, " +
                                            $"Warning: {Count(DiagnosticResultGravity.Warning)}, " +
                                            $"Info: {Count(DiagnosticResultGravity.Info)}");

                taskScope.Log($"Issues: {totalIssues}, Critical: {Count(DiagnosticResultGravity.Critical)}, Warning: {Count(DiagnosticResultGravity.Warning)}");
                await PublishDataChangedAsync(scope);
            }
        }
        catch (Exception ex)
        {
            taskScope.Item.Status = TaskItemStatus.Failed;
            taskScope.Log(ex.ToString(), LogLevel.Error);
            throw;
        }
    }
}
