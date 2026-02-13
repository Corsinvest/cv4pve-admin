/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Diagnostics;
using Corsinvest.ProxmoxVE.Admin.Core.Persistence;
using Corsinvest.ProxmoxVE.Admin.Core.Security.Auth;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.Components;

public partial class Maintenance(IAdminService adminService,
                                 ISettingsService settingsService,
                                 IBackgroundJobService backgroundJobService,
                                 ISystemLogService systemLogService,
                                 ModuleDbContext moduleDbContext,
                                 DialogService dialogService,
                                 IServiceScopeFactory serviceScopeFactory,
                                 IModuleService moduleService,
                                 IAuditService auditService)
{
    private bool IsFixAllBusy { get; set; }
    private bool IsCleanupAuditLogsBusy { get; set; }
    private bool IsDbReindexBusy { get; set; }
    private bool IsOptimizeDDatabaseBusy { get; set; }
    private bool IsClearMemoryCacheBusy { get; set; }
    private bool IsCleanupFailedJobsBusy { get; set; }
    private bool IsTestClusterConnectionsBusy { get; set; }
    private bool IsTestInternetConnectivityBusy { get; set; }
    private bool IsCleanupSystemLogsBusy { get; set; }

    private List<string> LogMessages { get; set; } = [];
    private int AuditLogRetentionDays { get; set; } = 180; // Default 6 months
    private int SystemLogsRetentionDays { get; set; } = 30; // Default 1 month

    private void AddLog(string message)
    {
        LogMessages.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
        StateHasChanged();
    }

    private void ClearLog()
    {
        LogMessages.Clear();
        StateHasChanged();
    }

    private static string GetLogStyle(string message)
    {
        if (message.Contains("] OK ")) { return "color: var(--rz-success);"; }
        return message.Contains("] FAIL ") ? "color: var(--rz-danger);" : "";
    }

    // Fix All
    private async Task FixAllAsync()
    {
        IsFixAllBusy = true;

        try
        {
            AddLog("Starting Fix All...");

            using var scope = serviceScopeFactory.CreateScope();

            foreach (var module in moduleService.Modules)
            {
                try
                {
                    await module.FixAsync(scope);
                    AddLog($"OK {module.Name}");
                }
                catch (Exception ex)
                {
                    AddLog($"FAIL {module.Name}: {ex.Message}");
                }
            }

            await CleanupAuditLogsAsync();
            await DbReindexAsync();
            await DbOptimizeAsync();
            await ClearMemoryCacheAsync();
            await CleanupFailedJobsAsync();

            AddLog("Fix All completed: tasks successful");
            await auditService.LogAsync("Maintenance.FixAll", true, "Completed tasks");
        }
        catch (Exception ex)
        {
            AddLog($"FAIL Fix All: {ex.Message}");
            await auditService.LogAsync("Maintenance.FixAll", false, ex.Message);
        }
        finally
        {
            IsFixAllBusy = false;
        }
    }

    // Database Maintenance
    private async Task CleanupAuditLogsAsync()
    {
        if (await dialogService.Confirm(
            L["Are you sure you want to delete audit logs older than {0} days?", AuditLogRetentionDays],
            L["Confirm Cleanup Audit Logs"]) == true)
        {
            IsCleanupAuditLogsBusy = true;
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-AuditLogRetentionDays);
                var deleted = await moduleDbContext.AuditLogs
                    .Where(a => a.Timestamp < cutoffDate)
                    .ExecuteDeleteAsync();

                AddLog($"OK Cleanup Audit Logs: deleted {deleted} record(s) older than {AuditLogRetentionDays} days");
                await auditService.LogAsync("Maintenance.CleanupAuditLogs", true, $"Deleted {deleted} audit logs older than {AuditLogRetentionDays} days");
            }
            catch (Exception ex)
            {
                AddLog($"FAIL Cleanup Audit Logs: {ex.Message}");
                await auditService.LogAsync("Maintenance.CleanupAuditLogs", false, ex.Message);
            }
            finally
            {
                IsCleanupAuditLogsBusy = false;
            }
        }
    }

    private async Task CleanupSystemLogsAsync()
    {
        if (await dialogService.Confirm(
            L["Are you sure you want to delete system logs older than {0} days?", SystemLogsRetentionDays],
            L["Confirm Cleanup System Logs"]) == true)
        {
            IsCleanupSystemLogsBusy = true;
            try
            {
                var deleted = await systemLogService.CleanupAsync(SystemLogsRetentionDays);

                AddLog($"OK Cleanup System Logs: deleted {deleted} record(s) older than {SystemLogsRetentionDays} days");
                await auditService.LogAsync("Maintenance.CleanupSystemLogs", true, $"Deleted {deleted} system logs older than {SystemLogsRetentionDays} days");
            }
            catch (Exception ex)
            {
                AddLog($"FAIL Cleanup System Logs: {ex.Message}");
                await auditService.LogAsync("Maintenance.CleanupSystemLogs", false, ex.Message);
            }
            finally
            {
                IsCleanupSystemLogsBusy = false;
            }
        }
    }

    private async Task DbReindexAsync()
    {
        if (await dialogService.Confirm(L["Are you sure you want to reindex the database?"], L["Confirm Database Reindex"]) == true)
        {
            using var scope = serviceScopeFactory.CreateScope();
            IsDbReindexBusy = true;
            try
            {
                AddLog("Starting database reindex...");
                foreach (var item in moduleService.Modules)
                {
                    try
                    {
                        await item.DatabaseMaintenanceAsync(scope, DatabaseMaintenanceOperation.Reindex);
                        AddLog($"OK Reindex {item.Name}");
                    }
                    catch (Exception ex)
                    {
                        AddLog($"FAIL Reindex {item.Name}: {ex.Message}");
                    }
                }
                AddLog("Database reindex completed");
                await auditService.LogAsync("Maintenance.DbReindex", true, "Completed");
            }
            catch (Exception ex)
            {
                AddLog($"FAIL Database Reindex: {ex.Message}");
                await auditService.LogAsync("Maintenance.DbReindex", false, ex.Message);
            }
            finally
            {
                IsDbReindexBusy = false;
            }
        }
    }

    private async Task DbOptimizeAsync()
    {
        if (await dialogService.Confirm(L["Are you sure you want to optimize the database?"], L["Confirm Database Optimize"]) == true)
        {
            using var scope = serviceScopeFactory.CreateScope();
            IsOptimizeDDatabaseBusy = true;
            try
            {
                AddLog("Starting database optimize...");
                foreach (var item in moduleService.Modules)
                {
                    try
                    {
                        await item.DatabaseMaintenanceAsync(scope, DatabaseMaintenanceOperation.Optimize);
                        AddLog($"OK Optimize {item.Name}");
                    }
                    catch (Exception ex)
                    {
                        AddLog($"FAIL Optimize {item.Name}: {ex.Message}");
                    }
                }
                AddLog("Database optimize completed");
                await auditService.LogAsync("Maintenance.DbOptimize", true, "Completed");
            }
            catch (Exception ex)
            {
                AddLog($"FAIL Database Optimize: {ex.Message}");
                await auditService.LogAsync("Maintenance.DbOptimize", false, ex.Message);
            }
            finally
            {
                IsOptimizeDDatabaseBusy = false;
            }
        }
    }

    // Cache Management
    private async Task ClearMemoryCacheAsync()
    {
        if (await dialogService.Confirm(L["Are you sure you want to clear the memory cache?"], L["Confirm Clear Memory Cache"]) == true)
        {
            IsClearMemoryCacheBusy = true;
            try
            {
                await Task.WhenAll(adminService.Select(item => item.CachedData.ClearCacheAsync().AsTask()));
                await PermissionService.ClearCacheAsync();
                await settingsService.ClearCacheAsync();
                AddLog("OK Clear Memory Cache");
                await auditService.LogAsync("Maintenance.ClearMemoryCache", true);
            }
            catch (Exception ex)
            {
                AddLog($"FAIL Clear Memory Cache: {ex.Message}");
                await auditService.LogAsync("Maintenance.ClearMemoryCache", false, ex.Message);
            }
            finally
            {
                IsClearMemoryCacheBusy = false;
            }
        }
    }

    // Job Management
    private async Task CleanupFailedJobsAsync()
    {
        if (await dialogService.Confirm(L["Are you sure you want to clean up the failed jobs?"], L["Confirm Cleanup Failed Jobs"]) == true)
        {
            IsCleanupFailedJobsBusy = true;
            try
            {
                var count = backgroundJobService.DeleteFails();

                if (count > 0)
                {
                    AddLog($"OK Cleanup Failed Jobs: deleted {count} job(s)");
                }
                else
                {
                    AddLog("OK Cleanup Failed Jobs: no failed jobs to delete");
                }
                await auditService.LogAsync("Maintenance.CleanupFailedJobs", true, $"Deleted {count} failed jobs");
            }
            catch (Exception ex)
            {
                AddLog($"FAIL Cleanup Failed Jobs: {ex.Message}");
                await auditService.LogAsync("Maintenance.CleanupFailedJobs", false, ex.Message);
            }
            finally
            {
                IsCleanupFailedJobsBusy = false;
            }
        }
    }

    private async Task TestClusterConnectionsAsync()
    {
        IsTestClusterConnectionsBusy = true;
        try
        {
            AddLog("Testing cluster connections...");

            var tasks = adminService.Select(async item =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    _ = await item.CachedData.GetPveClientAsync();
                    sw.Stop();
                    return (item.Settings.Name, Success: true, ElapsedMs: sw.ElapsedMilliseconds, Error: (string?)null);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return (item.Settings.Name, Success: false, ElapsedMs: sw.ElapsedMilliseconds, Error: ex.Message);
                }
            });

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.Success);
            var totalCount = results.Length;

            foreach (var (Name, Success, ElapsedMs, Error) in results)
            {
                if (Success)
                {
                    AddLog($"OK {Name} ({ElapsedMs}ms)");
                }
                else
                {
                    AddLog($"FAIL {Name} ({ElapsedMs}ms): {Error}");
                }
            }

            AddLog($"Test Cluster Connections completed: {successCount}/{totalCount} successful");
            await auditService.LogAsync("Maintenance.TestClusterConnections", successCount == totalCount, $"{successCount}/{totalCount} clusters connected");
        }
        catch (Exception ex)
        {
            AddLog($"FAIL Test Cluster Connections: {ex.Message}");
            await auditService.LogAsync("Maintenance.TestClusterConnections", false, ex.Message);
        }
        finally
        {
            IsTestClusterConnectionsBusy = false;
        }
    }

    private async Task TestInternetConnectivityAsync()
    {
        IsTestInternetConnectivityBusy = true;
        try
        {
            AddLog("Testing internet connectivity...");

            var testUrls = new[]
            {
                "https://www.google.com",
                "https://www.cloudflare.com",
                "https://www.github.com"
            };

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var tasks = testUrls.Select(async url =>
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                    sw.Stop();
                    return (Url: url, Success: response.IsSuccessStatusCode, ElapsedMs: sw.ElapsedMilliseconds, Error: (string?)null);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    return (Url: url, Success: false, ElapsedMs: sw.ElapsedMilliseconds, Error: ex.Message);
                }
            });

            var results = await Task.WhenAll(tasks);
            var successCount = results.Count(r => r.Success);
            var totalCount = results.Length;

            foreach (var (Url, Success, ElapsedMs, Error) in results)
            {
                if (Success)
                {
                    AddLog($"OK {Url} ({ElapsedMs}ms)");
                }
                else
                {
                    AddLog($"FAIL {Url} ({ElapsedMs}ms): {Error}");
                }
            }

            AddLog($"Test Internet Connectivity completed: {successCount}/{totalCount} successful");
            await auditService.LogAsync("Maintenance.TestInternetConnectivity", successCount == totalCount, $"{successCount}/{totalCount} endpoints reachable");
        }
        catch (Exception ex)
        {
            AddLog($"FAIL Test Internet Connectivity: {ex.Message}");
            await auditService.LogAsync("Maintenance.TestInternetConnectivity", false, ex.Message);
        }
        finally
        {
            IsTestInternetConnectivityBusy = false;
        }
    }
}
