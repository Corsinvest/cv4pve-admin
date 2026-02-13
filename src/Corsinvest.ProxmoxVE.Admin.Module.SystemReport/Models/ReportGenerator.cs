/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

public class ReportGenerator(PveClient client, ClusterClient clusterClient, JobResult job, ILogger logger)
{
    private record ComplianceMapping(string Code, string Name, string Section);

    private readonly Dictionary<string, (string Sheet, int Row)> _tablePositions = [];

    private IEnumerable<VmDiskInfo> _disks = [];

    #region Helpers

    //private bool IncludeConfig => options.ReportType is ReportType.Full or ReportType.ConfigOnly || IsComplianceReport;
    //private bool IncludeMetrics => options.ReportType is ReportType.Full or ReportType.MetricsOnly;
    //private bool IncludeSecurityData => options.ReportType is ReportType.Full || IsComplianceReport;
    //private bool IsComplianceReport => options.ReportType is ReportType.Compliance_ISO27001
    //                                                      or ReportType.Compliance_SOC2
    //                                                      or ReportType.Compliance_GDPR
    //                                                      or ReportType.Compliance_PCIDSS;

    #endregion

    public async Task<string> GenerateAsync(string outputPath)
    {
        _disks = await clusterClient.CachedData.GetDisksInfoAsync(false);
        var fileName = Path.Combine(outputPath, $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        await GenerateExcelAsync(fileName);
        return fileName;
    }

    private async Task GenerateExcelAsync(string fileName)
    {
        using var workbook = new XLWorkbook();
        ConfigureWorkbook(workbook);

        AddCoverPage(workbook);

        await AddClusterDataAsync(workbook);
        await AddStoragesDataAsync(workbook);
        await AddNodesDataAsync(workbook);
        await AddVmsDataAsync(workbook);
        AddComplianceMappingTables(workbook);

        workbook.SaveAs(fileName);
    }

    private void AddCoverPage(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Summary");
        var row = 1;

        // Titolo
        ws.Cell(row, 1).Value = "INFRASTRUCTURE REPORT";
        ws.Cell(row, 1).Style.Font.SetBold(true);
        ws.Cell(row, 1).Style.Font.SetFontSize(20);
        ws.Range(row, 1, row, 3).Merge();
        row += 2;

        // Info Report
        ws.Cell(row, 1).Value = "Report Information";
        ws.Cell(row, 1).Style.Font.SetBold(true);
        ws.Cell(row, 1).Style.Font.SetFontSize(14);
        row++;

        ws.Cell(row, 1).Value = "Generated:";
        ws.Cell(row, 2).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        row++;

        ws.Cell(row, 1).Value = "Report Type:";
        //ws.Cell(row, 2).Value = options.ReportType.ToString();
        row++;

        ws.Cell(row, 1).Value = "Application:";
        ws.Cell(row, 2).Value = "cv4pve-admin";
        row++;

        ws.Cell(row, 1).Value = "Version:";
        ws.Cell(row, 2).Value = Core.BuildInfo.Version;
        row += 2;

        // Filtri applicati
        ws.Cell(row, 1).Value = "Filters Applied";
        ws.Cell(row, 1).Style.Font.SetBold(true);
        ws.Cell(row, 1).Style.Font.SetFontSize(14);
        row++;

        ws.Cell(row, 1).Value = "Nodes:";
        ws.Cell(row, 2).Value = job.NodeNames;
        row++;

        ws.Cell(row, 1).Value = "VMs/Containers:";
        ws.Cell(row, 2).Value = job.VmIds;
        row++;

        ws.Cell(row, 1).Value = "Storages:";
        ws.Cell(row, 2).Value = job.StorageNames;
        row++;

        ws.Cell(row, 1).Value = "RRD TimeFrame:";
        ws.Cell(row, 2).Value = job.RrdDataTimeFrame.ToString();
        row++;

        ws.Cell(row, 1).Value = "RRD Consolidation:";
        ws.Cell(row, 2).Value = job.RrdDataConsolidation.ToString();
        row += 2;

        // Indice con link ai fogli
        ws.Cell(row, 1).Value = "Contents";
        ws.Cell(row, 1).Style.Font.SetBold(true);
        ws.Cell(row, 1).Style.Font.SetFontSize(14);
        row++;

        var sections = new[]
        {
            ("Cluster", "Cluster overview, users, roles, ACL, firewall, backup, replication"),
            ("Storages", "Storage list with links to details"),
            ("Nodes", "Node list with links to details"),
            ("Vms", "VM/Container list with links to details"),
            ("Compliance", "Compliance mapping tables")
        };

        foreach (var (sheetName, description) in sections)
        {
            ws.Cell(row, 1).Value = sheetName;
            ws.Cell(row, 1).Style.Font.SetUnderline(XLFontUnderlineValues.Single);
            ws.Cell(row, 1).Style.Font.SetFontColor(XLColor.Blue);
            ws.Cell(row, 1).SetHyperlink(new XLHyperlink($"'{sheetName}'!A1"));
            ws.Cell(row, 2).Value = description;
            row++;
        }

        row += 2;

        // Footer
        ws.Cell(row, 1).Value = "Generated by cv4pve-admin - Corsinvest Srl";
        ws.Cell(row, 1).Style.Font.SetItalic(true);
        ws.Cell(row, 1).Style.Font.SetFontColor(XLColor.Gray);

        ws.Column(1).Width = 20;
        ws.Column(2).Width = 60;
    }

    private void ConfigureWorkbook(XLWorkbook workbook)
    {
        workbook.Author = "cv4pve-admin";
        workbook.Properties.Author = "cv4pve-admin Corsinvest Srl";
        workbook.Properties.Title = "Infrastructure Report";
        workbook.Properties.Subject = "cv4pve-admin System Report";
        workbook.Properties.Category = "IT Infrastructure";
        workbook.Properties.Comments = "Automated report generated by cv4pve-admin tool for Proxmox VE";
        workbook.Properties.Company = "Corsinvest Srl";

        workbook.CustomProperties.Add("Application", "cv4pve-admin");
        workbook.CustomProperties.Add("Version", Core.BuildInfo.Version);
        workbook.CustomProperties.Add("GeneratedOn", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        //workbook.CustomProperties.Add("ReportType", options.ReportType.ToString());
    }

    #region Helper Methods

    private static bool CheckNames(string names, string name)
    {
        if (string.IsNullOrWhiteSpace(names) || names.Equals("@all", StringComparison.OrdinalIgnoreCase)) { return true; }

        foreach (var token in names.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (string.IsNullOrWhiteSpace(token)) { continue; }
            if (!token.Contains('*'))
            {
                if (token.Equals(name, StringComparison.OrdinalIgnoreCase)) { return true; }
            }
            else
            {
                if (token == "*") { return true; }

                if (token.StartsWith('*') && token.Count(c => c == '*') == 1)
                {
                    if (name.EndsWith(token[1..], StringComparison.OrdinalIgnoreCase)) { return true; }
                    continue;
                }

                if (token.EndsWith('*') && token.Count(c => c == '*') == 1)
                {
                    if (name.StartsWith(token[..^1], StringComparison.OrdinalIgnoreCase)) { return true; }
                    continue;
                }

                var pattern = "^" + Regex.Escape(token).Replace(@"\*", ".*") + "$";
                if (Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase)) { return true; }
            }
        }

        return false;
    }

    private int CreateTable<T>(IXLWorksheet ws, int row, string title, IEnumerable<T> data)
    {
        _tablePositions[$"{ws.Name}:{title}"] = (ws.Name, row);

        ws.Cell(row, 1).Value = title;
        ws.Cell(row, 1).Style.Font.SetBold(true);
        row++;
        var table = ws.Cell(row, 1).InsertTable(data, true);
        table.AutoFilter.IsEnabled = true;
        return table.RowCount() + 2;
    }

    private static string FormatSnapshotSize(double value)
        => value == -1
            ? string.Empty
            : FormatHelper.FromBytes(value);

    #endregion

    #region Cluster Data

    private async Task AddClusterDataAsync(XLWorkbook workbook)
    {
        var row = 1;
        var ws = workbook.Worksheets.Add("Cluster");

        // Status
        row += CreateTable(ws,
                           row,
                           "Status",
                           (await client.Cluster.Status.GetAsync())
                            .Select(a => new
                            {
                                a.Id,
                                a.Name,
                                a.Type,
                                a.Nodes,
                                a.Version,
                                a.Quorate,
                                Level = NodeHelper.DecodeLevelSupport(a.Level),
                                a.IpAddress,
                                a.NodeId,
                                a.IsOnline
                            }));

        var snapshotsUsage = new List<dynamic>();
        foreach (var forReplication in new[] { false, true })
        {
            snapshotsUsage.AddRange(_disks.GroupBy(a => new { a.Type, a.Host, a.SpaceName })
                                         .Select(a => new
                                         {
                                             a.Key.Type,
                                             a.Key.Host,
                                             a.Key.SpaceName,
                                             Replication = forReplication,
                                             Count = a.SelectMany(b => b.Snapshots.Where(c => c.Replication, forReplication)).Count(),
                                             Size = a.SelectMany(b => b.Snapshots.Where(c => c.Replication, forReplication)).Sum(b => b.Size),
                                             Vms = a.Where(b => b.Snapshots.Where(c => c.Replication, forReplication).Any())
                                               .DistinctBy(a => a.VmId)
                                               .Count()
                                         }));
        }
        row += CreateTable(ws, row, "Snapshots Usage", snapshotsUsage);

        var users = await client.Access.Users.GetAsync(full: true);

        row += CreateTable(ws,
                           row,
                           "Users",
                           users.Select(a => new
                           {
                               a.Id,
                               Enable = a.Enable == 1,
                               a.Email,
                               Expire = DateTimeOffset.FromUnixTimeSeconds(a.Expire).DateTime
                           }));

        row += CreateTable(ws,
                           row,
                           "API Tokens",
                           users.SelectMany(a => a.Tokens).Select(a => new
                           {
                               a.Id,
                               Expire = DateTimeOffset.FromUnixTimeSeconds(a.Expire).DateTime,
                               PrivSeparated = a.Privsep == 1,
                               a.Comment
                           }));

        row += CreateTable(ws,
                           row,
                           "Two-Factor Authentication Status",
                           (await client.Access.Tfa.GetAsync()).Select(t => new
                           {
                               User = t.UserId,
                               TfaTypes = string.Join(", ", t.Entries?.Select(e => e.Type).Distinct() ?? []),
                               TfaCount = t.Entries?.Count() ?? 0
                           }));

        row += CreateTable(ws,
                           row,
                           "Groups",
                           (await client.Access.Groups.GetAsync()).Select(a => new
                           {
                               a.Id,
                               a.Users,
                               a.Comment
                           }));

        row += CreateTable(ws,
                           row,
                           "Roles",
                           (await client.Access.Roles.GetAsync()).Select(a => new
                           {
                               a.Id,
                               Privileges = a.Privileges.Replace(",", Environment.NewLine),
                               Special = a.Special == 1
                           }));

        row += CreateTable(ws,
                           row,
                           "Acl",
                           (await client.Access.Acl.GetAsync()).Select(a => new
                           {
                               Id = a.Roleid,
                               a.Path,
                               UsersOrGroup = a.UsersGroupid,
                               Propagate = a.Propagate == 1,
                               a.Type
                           }));

        row += AddFirewallRules(ws, row, await client.Cluster.Firewall.Rules.GetAsync());

        var fwOptions = await client.Cluster.Firewall.Options.GetAsync();
        row += CreateTable(ws,
                           row,
                           "Firewall Options",
                           [
                               new
                                   {
                                       fwOptions.Enable,
                                       fwOptions.PolicyIn,
                                       fwOptions.PolicyOut,
                                       fwOptions.LogRatelimit
                                   }
                           ]);

        row += CreateTable(ws,
                           row,
                           "Domains",
                           (await client.Access.Domains.GetAsync()).Select(a => new
                           {
                               a.Type,
                               a.Realm,
                               a.Comment
                           }));

        row += CreateTable(ws,
                           row,
                           "Backup",
                           (await client.Cluster.Backup.GetAsync()).Select(a => new
                           {
                               a.Id,
                               a.Enabled,
                               a.All,
                               VmId = a.VmId.Replace(",", Environment.NewLine),
                               a.Mode,
                               a.Storage,
                               a.StartTime,
                               a.Mailto,
                               a.Pool,
                               a.DayOfWeek,
                               a.MailNotification,
                               a.Quiet,
                               a.Compress,
                               a.Type,
                               a.Schedule,
                               a.NotesTemplate,
                               NextRun = DateTimeOffset.FromUnixTimeSeconds(a.NextRun).DateTime,
                               a.Node
                           }));

        CreateTable(ws,
                    row,
                    "Replication",
                    (await client.Cluster.Replication.GetAsync()).Select(a => new
                    {
                        a.Id,
                        a.Schedule,
                        a.Type,
                        a.Guest,
                        a.JobNum,
                        a.Source,
                        a.Target,
                        a.Disable,
                        a.Rate
                    }));

        ws.Columns().AdjustToContents();
    }

    #endregion

    #region Storage Data

    private async Task AddStoragesDataAsync(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Storages");
        var resources = await clusterClient.CachedData.GetResourcesAsync(false);
        var items = new List<dynamic>();

        foreach (var item in resources.Where(a => a.ResourceType == ClusterResourceType.Storage)
                                      .OrderBy(a => a.Node))
        {
            if (!CheckNames(job.StorageNames, item.Storage)) { continue; }

            items.Add(new
            {
                item.Id,
                item.Node,
                item.Storage,
                item.Status,
                item.PluginType,
                Content = item.Content.Replace(",", Environment.NewLine),
                item.Shared,
                item.DiskSize,
                DiskSizeFormatted = FormatHelper.FromBytes(item.DiskSize),
                item.DiskUsage,
                DiskUsageFormatted = FormatHelper.FromBytes(item.DiskUsage),
                item.DiskUsagePercentage
            });

            await AddStorageDetailAsync(workbook, item.Node, item.Storage);
        }

        var row = 1;
        row += CreateTable(ws, row, "Storages", items);

        // Link to sheets
        for (var i = 0; i < items.Count; i++)
        {
            ws.Cell(i + 3, 1).SetHyperlink(new XLHyperlink($"'{CreateSheetNameStorage(items[i].Node, items[i].Storage)}'!A1"));
        }

        ws.Columns().AdjustToContents();
    }

    private static string CreateSheetNameStorage(string node, string storage) => $"Storage {node} - {storage}";

    private async Task AddStorageDetailAsync(XLWorkbook workbook, string node, string storage)
    {
        var row = 1;
        var ws = workbook.Worksheets.Add(CreateSheetNameStorage(node, storage));

        row += CreateTable(ws,
                           row,
                           "Content",
                           (await client.Nodes[node].Storage[storage].Content.GetAsync())
                           .Select(a => new
                           {
                               a.Content,
                               a.ContentDescription,
                               a.CreationDate,
                               a.Encrypted,
                               a.FileName,
                               a.Format,
                               a.Name,
                               a.Notes,
                               a.Protected,
                               a.Size,
                               SizeFormatted = FormatHelper.FromBytes(a.Size),
                               a.Storage,
                               a.Verified,
                               a.VmId
                           }));

        CreateTable(ws,
                    row,
                    "RRD Data",
                    (await clusterClient.CachedData.GetRrdDataAsync(node, storage, job.RrdDataTimeFrame, job.RrdDataConsolidation, false))
                        .Select(a => new
                        {
                            a.TimeDate,
                            a.Used,
                            a.Size
                        }));

        ws.Columns().AdjustToContents();
    }

    #endregion

    #region Node Data

    private async Task AddNodesDataAsync(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Nodes");
        var resources = await clusterClient.CachedData.GetResourcesAsync(false);
        var items = new List<dynamic>();

        foreach (var item in resources.Where(a => a.ResourceType == ClusterResourceType.Node)
                                      .OrderBy(a => a.Node))
        {
            if (!CheckNames(job.NodeNames, item.Node)) { continue; }

            var status = await client.Nodes[item.Node].Status.GetAsync();
            var version = await client.Nodes[item.Node].Version.GetAsync();
            var subscription = await client.Nodes[item.Node].Subscription.GetAsync();

            var includeSnapshotsSize = true;

            var snapshotsSize = includeSnapshotsSize
                                    ? DiskInfoHelper.CalculateSnapshots(item.Node, _disks, false)
                                    : -1;

            var snapshotsSizeForReplication = includeSnapshotsSize
                                                ? DiskInfoHelper.CalculateSnapshots(item.Node, _disks, true)
                                                : -1;

            items.Add(new
            {
                item.Id,
                item.Node,
                item.Status,
                item.CpuSize,
                item.MemorySize,
                MemorySizeFormatted = FormatHelper.FromBytes(item.MemorySize),
                item.MemoryUsage,
                MemoryUsageFormatted = FormatHelper.FromBytes(item.MemoryUsage),
                item.MemoryUsagePercentage,
                item.DiskSize,
                DiskSizeFormatted = FormatHelper.FromBytes(item.DiskSize),
                item.DiskUsagePercentage,
                Uptime = FormatHelper.UptimeInfo(item.Uptime),
                item.CgroupMode,
                item.NodeLevel,
                CpuCpus = status.CpuInfo.Cpus,
                CpuModel = status.CpuInfo.Model,
                CpuMhz = status.CpuInfo.Mhz,
                CpuCores = status.CpuInfo.Cores,
                CpuSockets = status.CpuInfo.Sockets,
                SwapTotal = status.Swap.Total,
                SwapTotalFormatted = FormatHelper.FromBytes(status.Swap.Total),
                SwapUsed = status.Swap.Used,
                SwapUsedFormatted = FormatHelper.FromBytes(status.Swap.Used),
                status.PveVersion,
                RootFsTotal = status.RootFs.Total,
                RootFsTotalFormatted = FormatHelper.FromBytes(status.RootFs.Total),
                RootFsUsed = status.RootFs.Used,
                RootFsUsedFormatted = FormatHelper.FromBytes(status.RootFs.Used),
                KsmShared = status.Ksm.Shared,
                KernelVersion = status.Kversion,
                SubscriptionProductName = subscription.ProductName,
                SubscriptionRegDate = subscription.RegDate,
                VersionRelease = version.Release,
                VersionVersion = version.Version,
                SnapshotsSize = snapshotsSize,
                SnapshotsSizeFormatted = FormatSnapshotSize(snapshotsSize),
                SnapshotsSizeForReplication = snapshotsSizeForReplication,
                SnapshotsSizeForReplicationFormatted = FormatSnapshotSize(snapshotsSizeForReplication)
            });

            await AddNodeDetailAsync(workbook, item.Node);
        }

        var row = 1;
        row += CreateTable(ws, row, "Nodes", items);

        // Link to sheets
        for (var i = 0; i < items.Count; i++)
        {
            ws.Cell(i + 3, 1).SetHyperlink(new XLHyperlink($"'{CreateSheetNameNode(items[i].Node)}'!A1"));
        }

        ws.Columns().AdjustToContents();
    }

    private static string CreateSheetNameNode(string node) => $"Node {node}";

    private async Task AddNodeDetailAsync(XLWorkbook workbook, string node)
    {
        var row = 1;
        var ws = workbook.Worksheets.Add(CreateSheetNameNode(node));

        row += CreateTable(ws,
                           row,
                           "Services",
                           (await client.Nodes[node].Services.GetAsync())
                           .Select(a => new
                           {
                               a.Name,
                               a.State,
                               a.IsRunning,
                               a.Service,
                               a.Description
                           }));

        row += CreateTable(ws,
                           row,
                           "Network",
                           (await client.Nodes[node].Network.GetAsync())
                           .Select(a => new
                           {
                               a.Active,
                               a.Type,
                               a.Interface,
                               a.Method,
                               a.Method6,
                               a.Gateway,
                               a.Priority,
                               a.BondMode,
                               a.Address,
                               a.Netmask,
                               a.Cidr,
                               a.Comments,
                               Families = a.Families.JoinAsString(Environment.NewLine),
                               a.BondMiimon,
                               a.Slaves,
                               AutoStart = a.AutoStart == 1,
                               a.BondPrimary,
                               a.BridgeStp,
                               a.BridgeVlanAware,
                               a.BridgeVids,
                               a.BridgeFd,
                               a.BridgePorts,
                               a.Exists
                           }));

        var disksData = await client.Nodes[node].Disks.List.GetAsync();
        row += CreateTable(ws,
                           row,
                           "Disks",
                           disksData.Select(a => new
                           {
                               a.Used,
                               DevicePath = a.DevPath,
                               a.Vendor,
                               a.Serial,
                               a.Type,
                               a.Model,
                               a.Wwn,
                               a.Health,
                               a.ByIdLink,
                               a.Gpt,
                               a.Wearout,
                               a.Rpm,
                               a.OsdId,
                               a.Size,
                               SizeFormatted = FormatHelper.FromBytes(a.Size)
                           }));

        // SMART data
        var smartData = new Dictionary<string, NodeDiskSmart>();
        foreach (var diskItem in disksData) { smartData[diskItem.DevPath] = await client.GetDiskSmart(node, diskItem.DevPath); }
        row += CreateTable(ws,
                           row,
                           "DiskSmarts",
                           smartData.SelectMany(a => a.Value.Attributes.Select(attr => new
                           {
                               DevicePath = a.Key,
                               attr.Id,
                               attr.Name,
                               attr.Value,
                               attr.Worst,
                               attr.Threshold,
                               attr.Raw,
                               attr.Fail,
                               attr.Flags
                           })));

        row += await AddReplicationAsync(ws, row, node, null);

        row += CreateTable(ws,
                           row,
                           "RRD Data",
                           (await clusterClient.CachedData.GetRrdDataAsync(node, job.RrdDataTimeFrame, job.RrdDataConsolidation, false))
                                .Select(a => new
                                {
                                    a.TimeDate,
                                    a.NetIn,
                                    a.NetOut,
                                    a.CpuUsagePercentage,
                                    a.IoWait,
                                    a.Loadavg,
                                    a.MemorySize,
                                    a.MemoryUsage,
                                    a.MemoryUsagePercentage,
                                    a.SwapSize,
                                    a.SwapUsage,
                                    a.RootSize,
                                    a.RootUsage
                                }));

        row += CreateTable(ws,
                           row,
                           "Apt Update",
                           (await client.Nodes[node].Apt.Update.GetAsync()).Select(a => new
                           {
                               a.Arch,
                               a.Origin,
                               a.Section,
                               a.Package,
                               a.Priority,
                               a.Version,
                               a.OldVersion,
                               a.Title,
                               a.NotifyStatus,
                               a.Description
                           }));

        row += CreateTable(ws,
                           row,
                           "Package Version",
                           (await client.Nodes[node].Apt.Versions.GetAsync()).Select(a => new
                           {
                               a.Arch,
                               a.Origin,
                               a.Section,
                               a.Package,
                               a.Priority,
                               a.Version,
                               a.OldVersion,
                               a.Title,
                               a.CurrentState,
                               a.Description
                           }));

        row += AddFirewallRules(ws, row, await client.Nodes[node].Firewall.Rules.GetAsync());

        row += CreateTable(ws,
                           row,
                           "SSL Certificates",
                           (await client.Nodes[node].Certificates.Info.GetAsync()).Select(cert => new
                           {
                               cert.FileName,
                               cert.Subject,
                               cert.Issuer,
                               NotBefore = DateTimeOffset.FromUnixTimeSeconds(cert.NotBefore).DateTime,
                               NotAfter = DateTimeOffset.FromUnixTimeSeconds(cert.NotAfter).DateTime,
                               DaysUntilExpiry = (DateTimeOffset.FromUnixTimeSeconds(cert.NotAfter).DateTime - DateTime.UtcNow).Days,
                           }));

        ws.Columns().AdjustToContents();
    }

    private int AddFirewallRules(IXLWorksheet ws, int row, IEnumerable<Api.Shared.Models.Common.FirewallRule> rules)
        => CreateTable(ws,
                       row,
                       "Firewall Rules",
                       rules.Select(r => new
                       {
                           r.Positon,
                           r.Type,
                           r.Action,
                           r.Enable,
                           r.Source,
                           r.Dest,
                           r.Protocol,
                           r.DestinationPort,
                           r.SourcePort,
                           r.Comment
                       }));

    private async Task<int> AddReplicationAsync(IXLWorksheet ws, int row, string node, long? vmId)
        => CreateTable(ws,
                       row,
                       "Replication",
                       (await clusterClient.CachedData.GetReplicationsAsync(node, vmId, false))
                        .Select(a => new
                        {
                            a.Disable,
                            a.Id,
                            a.VmType,
                            a.Type,
                            a.Guest,
                            a.Source,
                            a.Target,
                            a.Schedule,
                            a.FailCount,
                            a.JobNum,
                            a.Duration,
                            LastSync = DateTimeOffset.FromUnixTimeSeconds(a.LastSync).DateTime,
                            NextSync = DateTimeOffset.FromUnixTimeSeconds(a.NextSync).DateTime,
                            LastTry = DateTimeOffset.FromUnixTimeSeconds(a.LastTry).DateTime,
                            a.Comment,
                            a.Error,
                            a.Rate
                        }));

    #endregion

    #region VM Data

    private async Task AddVmsDataAsync(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Vms");
        var resources = (await clusterClient.CachedData.GetResourcesAsync(false))
                            .Where(a => a.ResourceType == ClusterResourceType.Vm)
                            .OrderBy(a => a.Node)
                            .ThenBy(a => a.Type)
                            .ThenBy(a => a.VmId)
                            .ToList();

        var items = new List<dynamic>();

        foreach (var item in resources)
        {
            if (!CheckNames(job.VmIds, item.VmId.ToString())) { continue; }

            var config = await client.GetVmConfigAsync(item.Node, item.VmType, item.VmId);
            var cnfigQemu = config as VmConfigQemu;
            var configLxc = config as VmConfigLxc;

            var qemuAgentRunning = false;
            var hostname = string.Empty;
            var osInfo = string.Empty;
            var ipAddresses = string.Empty;
            VmQemuAgentNetworkGetInterfaces? qemuAgentNetworkGetInterfaces = null;

            if (item.VmType == VmType.Qemu && item.IsRunning)
            {
                var vm = client.Nodes[item.Node].Qemu[item.VmId];
                var ping = await vm.Agent.Ping.Ping();
                qemuAgentRunning = ping.IsSuccessStatusCode;

                try { hostname = (await vm.Agent.GetHostName.GetAsync())?.Result?.HostName; }
                catch (Exception ex) { logger.LogWarning(ex, "Failed to get hostname from QEMU agent for VM {VmId}", item.VmId); }

                try { osInfo = (await vm.Agent.GetOsinfo.GetAsync())?.Result?.PrettyName; }
                catch (Exception ex) { logger.LogWarning(ex, "Failed to get OS info from QEMU agent for VM {VmId}", item.VmId); }

                try
                {
                    qemuAgentNetworkGetInterfaces = await client.Nodes[item.Node].Qemu[item.VmId].Agent.NetworkGetInterfaces.GetAsync();
                    if (qemuAgentNetworkGetInterfaces?.Result.Any() == true)
                    {
                        ipAddresses = qemuAgentNetworkGetInterfaces.Result
                                                                   .Where(a => !string.IsNullOrEmpty(a.HardwareAddress)
                                                                                && a.HardwareAddress != "00:00:00:00:00:00"
                                                                                && a.HardwareAddress != "0:0:0:0:0:0")
                                                                   .SelectMany(a => a.IpAddresses)
                                                                   .Select(a => $"{a.IpAddress}/{a.Prefix}")
                                                                   .JoinAsString(Environment.NewLine);
                    }
                }
                catch (Exception ex) { logger.LogWarning(ex, "Failed to get network interfaces from QEMU agent for VM {VmId}", item.VmId); }
            }
            else if (item.VmType == VmType.Lxc)
            {
                hostname = configLxc!.Hostname;
                ipAddresses = configLxc.Networks
                                       .Select(a => a.IpAddress)
                                       .JoinAsString(Environment.NewLine);
            }

            var includeSnapshotsSize = true;

            var snapshotsSize = includeSnapshotsSize
                                    ? DiskInfoHelper.CalculateSnapshots(item.Node, item.VmId, _disks, false)
                                    : -1;

            var snapshotsSizeForReplication = includeSnapshotsSize
                                                ? DiskInfoHelper.CalculateSnapshots(item.Node, item.VmId, _disks, true)
                                                : -1;

            items.Add(new
            {
                item.Id,
                item.Node,
                item.VmId,
                item.Name,
                item.Description,
                item.Type,
                item.Status,
                item.CpuSize,
                item.CpuUsagePercentage,
                item.HostCpuUsage,
                item.MemorySize,
                MemorySizeFormatted = FormatHelper.FromBytes(item.MemorySize),
                item.MemoryUsage,
                MemoryUsageFormatted = FormatHelper.FromBytes(item.MemoryUsage),
                item.MemoryUsagePercentage,
                item.HostMemoryUsage,
                item.DiskSize,
                DiskSizeFormatted = FormatHelper.FromBytes(item.DiskSize),
                item.DiskUsage,
                DiskUsageFormatted = FormatHelper.FromBytes(item.DiskUsage),
                item.DiskUsagePercentage,
                Uptime = FormatHelper.UptimeInfo(item.Uptime),
                Hostname = hostname,
                OsInfo = osInfo,
                IpAddresses = ipAddresses,
                SnapshotsSize = snapshotsSize,
                SnapshotsSizeFormatted = FormatSnapshotSize(snapshotsSize),
                SnapshotsSizeForReplication = snapshotsSizeForReplication,
                SnapshotsSizeForReplicationFormatted = FormatSnapshotSize(snapshotsSizeForReplication),
                ConfigOnBoot = config.OnBoot,
                ConfigArch = config.Arch,
                ConfigOsType = config.OsType,
                ConfigOsTypeDecode = config.OsTypeDecode,
                ConfigProtection = config.Protection,
                ConfigVmOsType = config.VmOsType.ToString(),
                LxcCores = configLxc?.Cores,
                LxcNameserver = configLxc?.Nameserver,
                LxcSearchDomain = configLxc?.SearchDomain,
                LxcSwap = configLxc?.Swap,
                LxcUnprivileged = configLxc?.Unprivileged,
                QemuAgentRunning = qemuAgentRunning,
                QemuAcpi = cnfigQemu?.Acpi,
                QemuAgentEnabled = cnfigQemu?.AgentEnabled,
                QemuArgs = cnfigQemu?.Args,
                QemuAudio0 = cnfigQemu?.Audio0,
                QemuBalloon = (cnfigQemu?.Balloon ?? 0) == 1,
                QemuBios = cnfigQemu?.Bios,
                QemuBoot = cnfigQemu?.Boot,
                QemuBootDisk = cnfigQemu?.BootDisk,
                QemuCores = cnfigQemu?.Cores,
                QemuCpu = cnfigQemu?.Cpu,
                QemuDescription = cnfigQemu?.Description,
                QemuKeyboard = cnfigQemu?.Keyboard,
                QemuKvm = cnfigQemu?.Kvm,
                QemuLocaltime = cnfigQemu?.Localtime,
                QemuName = cnfigQemu?.Name,
                QemuNuma = cnfigQemu?.Numa,
                QemuScsiHw = cnfigQemu?.ScsiHw,
                QemuSearchDomain = cnfigQemu?.SearchDomain,
                QemuShares = cnfigQemu?.Shares,
                QemuSmbios1 = cnfigQemu?.Smbios1,
                QemuSockets = cnfigQemu?.Sockets,
                QemuStartUp = cnfigQemu?.StartUp,
                QemuTablet = cnfigQemu?.Tablet,
                QemuVga = cnfigQemu?.Vga
            });

            await AddVmDetailAsync(workbook, item, config, qemuAgentNetworkGetInterfaces);
        }

        var row = 1;
        row += CreateTable(ws, row, "Vms", items);

        // Link to sheets
        for (var i = 0; i < items.Count; i++)
        {
            ws.Cell(i + 3, 1).SetHyperlink(new XLHyperlink($"'{CreateSheetNameVm(items[i].Description)}'!A1"));
        }

        ws.Columns().AdjustToContents();
    }

    private static string CreateSheetNameVm(string description) => $"VM {description}";

    private async Task AddVmDetailAsync(XLWorkbook workbook,
                                        ClusterResource vm,
                                        VmConfig config,
                                        VmQemuAgentNetworkGetInterfaces? qemuAgentNetworkGetInterfaces)
    {
        var row = 1;
        var ws = workbook.Worksheets.Add(CreateSheetNameVm(vm.Description));

        row += CreateTable(ws,
                           row,
                           "Network",
                           config.Networks.Select(a => new
                           {
                               a.Name,
                               a.Bridge,
                               a.Type,
                               a.Queues,
                               a.Tag,
                               a.Firewall,
                               a.Gateway,
                               a.IpAddress,
                               a.IpAddress6,
                               a.Gateway6,
                               a.MacAddress,
                               a.Model,
                               a.Rate,
                               a.Disconnect,
                               a.Trunks,
                               a.Mtu,
                               a.LinkDown
                           }));

        row += CreateTable(ws,
                           row,
                           "Disks",
                           config.Disks.Select(a => new
                           {
                               a.Id,
                               a.Storage,
                               a.FileName,
                               a.Device,
                               a.Passthrough,
                               a.MountPoint,
                               a.MountSourcePath,
                               a.Size,
                               a.Backup
                           }));

        if (vm.VmType == VmType.Qemu && vm.IsRunning)
        {
            try
            {
                var fs = await client.Nodes[vm.Node].Qemu[vm.VmId].Agent.GetFsinfo.GetAsync();
                if (fs?.Result.Any() == true)
                {
                    row += CreateTable(ws,
                                       row,
                                       "Qemu Guest Info File Systems",
                                       fs.Result.Select(a => new
                                       {
                                           a.Name,
                                           a.MountPoint,
                                           a.Type,
                                           a.UsedBytes,
                                           a.TotalBytes,
                                           Disks = a.Disks.Select(d => $"{d.Dev} - {d.BusType} {d.Target}:{d.Bus}").JoinAsString(Environment.NewLine)
                                       }));
                }
            }
            catch (Exception ex) { logger.LogWarning(ex, "Failed to get filesystem info from QEMU agent for VM {VmId}", vm.VmId); }

            if (qemuAgentNetworkGetInterfaces?.Result.Any() == true)
            {
                row += CreateTable(ws,
                                   row,
                                   "Qemu Guest Info Network",
                                   qemuAgentNetworkGetInterfaces.Result.Select(a => new
                                   {
                                       a.Name,
                                       a.HardwareAddress,
                                       IpAddresses = a.IpAddresses.Select(ip => ip.IpAddress + "/" + ip.Prefix).JoinAsString(Environment.NewLine)
                                   }));
            }
        }

        row += CreateTable(ws,
                           row,
                          "RRD Data",
                          (await clusterClient.CachedData.GetRrdDataAsync(vm.Node,
                                                                          vm.VmType,
                                                                          vm.VmId,
                                                                          job.RrdDataTimeFrame,
                                                                          job.RrdDataConsolidation,
                                                                          false))
                            .Select(a => new
                            {
                                a.TimeDate,
                                a.NetIn,
                                a.NetOut,
                                a.CpuUsagePercentage,
                                a.MemorySize,
                                a.MemoryUsage,
                                a.MemoryUsagePercentage,
                                a.DiskUsage,
                                a.DiskSize,
                                a.DiskUsagePercentage,
                                a.DiskRead,
                                a.DiskWrite
                            }));

        row += CreateTable(ws,
                               row,
                               "Backup",
                               (await client.Nodes[vm.Node].GetBackupsInAllStoragesAsync(Convert.ToInt32(vm.VmId)))
                                .Select(a => new
                                {
                                    a.ContentDescription,
                                    a.CreationDate,
                                    a.Encrypted,
                                    a.FileName,
                                    a.Format,
                                    a.Name,
                                    a.Notes,
                                    a.Protected,
                                    a.Size,
                                    SizeFormatted = FormatHelper.FromBytes(a.Size),
                                    a.Storage,
                                    a.Verified,
                                    a.VmId
                                }));

        row += await AddReplicationAsync(ws, row, vm.Node, vm.VmId);

        row += CreateTable(ws,
                           row,
                           "Snapshots",
                           (await clusterClient.CachedData.GetSnapshotsAsync(vm.Node,
                                                                             vm.VmType,
                                                                             vm.VmId,
                                                                             false))
                            .Select(a => new
                            {
                                a.Name,
                                a.Description,
                                a.Parent,
                                a.VmStatus,
                                a.Date,
                                Size = DiskInfoHelper.CalculateSnapshots(vm.VmId, a.Name, _disks)
                            }));

        row += AddFirewallRules(ws, row, await client.Nodes[vm.Node].Qemu[vm.VmId].Firewall.Rules.GetAsync());

        var fwOptions = await client.Nodes[vm.Node].Qemu[vm.VmId].Firewall.Options.GetAsync();
        row += CreateTable(ws,
                           row,
                           "Firewall Options",
                           [
                               new
                                   {
                                       fwOptions.Enable,
                                       fwOptions.Ipfilter,
                                       fwOptions.MacFilter,
                                       fwOptions.PolicyOut,
                                   }
                           ]);

        ws.Columns().AdjustToContents();
    }
    #endregion

    #region Compliance Mapping

    private void AddComplianceMappingTables(XLWorkbook workbook)
    {
        var ws = workbook.Worksheets.Add("Compliance");
        var row = 1;

        // Header con tipo compliance
        ws.Cell(row, 1).Value = "Report: ";// {options.ReportType}";
        ws.Cell(row, 1).Style.Font.SetBold(true);
        ws.Cell(row, 1).Style.Font.SetFontSize(14);
        row += 2;

        //switch (options.ReportType)
        //{
        //    case ReportType.Compliance_ISO27001: AddIso27001Sections(ws, ref row); break;
        //    case ReportType.Compliance_SOC2: AddSoc2Sections(ws, ref row); break;
        //    case ReportType.Compliance_GDPR: AddGdprSections(ws, ref row); break;
        //    case ReportType.Compliance_PCIDSS: AddPciDssSections(ws, ref row); break;
        //}

        row++;
        ws.Cell(row, 1).Value = "This report provides a technical overview of infrastructure-level security controls observable on the Proxmox VE environment. Organizational, procedural, and human-resource controls are outside the scope of this report.";

        ws.Columns().AdjustToContents();
    }

    private void AddComplianceMappingSection(IXLWorksheet ws,
                                                    ref int row,
                                                    string title,
                                                    IEnumerable<ComplianceMapping> mappings)
    {
        ws.Cell(row, 1).Value = title;
        ws.Cell(row, 1).Style.Font.SetBold(true);
        row++;

        // Crea tabella manualmente per aggiungere link
        var tableStartRow = row;
        ws.Cell(row, 1).Value = "Code";
        ws.Cell(row, 2).Value = "Name";
        ws.Cell(row, 3).Value = "Section";
        ws.Range(row, 1, row, 3).Style.Font.SetBold(true);
        ws.Range(row, 1, row, 3).Style.Fill.SetBackgroundColor(XLColor.LightGray);
        row++;

        foreach (var mapping in mappings)
        {
            ws.Cell(row, 1).Value = mapping.Code;
            ws.Cell(row, 2).Value = mapping.Name;

            // Estrae riferimenti alle tabelle dal testo Section (es. "See: Users, Roles, ACL tables")
            var sectionText = mapping.Section;
            ws.Cell(row, 3).Value = sectionText;

            // Cerca link alle tabelle note
            var tableLinks = ExtractTableLinks(sectionText);
            if (tableLinks.Any())
            {
                // Primo link disponibile
                var firstLink = tableLinks.First();
                ws.Cell(row, 3).Style.Font.SetUnderline(XLFontUnderlineValues.Single);
                ws.Cell(row, 3).Style.Font.SetFontColor(XLColor.Blue);
                ws.Cell(row, 3).SetHyperlink(new XLHyperlink($"'{firstLink.Sheet}'!A{firstLink.Row}"));
            }

            row++;
        }

        // Formatta come tabella
        var tableRange = ws.Range(tableStartRow, 1, row - 1, 3);
        tableRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        tableRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

        row += 2;
    }

    /// <summary>
    /// Estrae link alle tabelle dal testo della sezione compliance
    /// </summary>
    private IEnumerable<(string Sheet, int Row)> ExtractTableLinks(string sectionText)
    {
        var result = new List<(string Sheet, int Row)>();

        // Mapping delle parole chiave alle tabelle
        var keywordToTable = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["Users"] = ["Cluster:Users"],
            ["Roles"] = ["Cluster:Roles"],
            ["ACL"] = ["Cluster:Acl"],
            ["API Tokens"] = ["Cluster:API Tokens"],
            ["TFA"] = ["Cluster:Two-Factor Authentication Status"],
            ["Certificates"] = ["Cluster:SSL Certificates"],
            ["Firewall Rules"] = ["Cluster:Firewall Rules"],
            ["Firewall Options"] = ["Cluster:Firewall Options"],
            ["Backup"] = ["Cluster:Backup"],
            ["Replication"] = ["Cluster:Replication"],
            ["Groups"] = ["Cluster:Groups"],
            ["Domains"] = ["Cluster:Domains"],
            ["Services Status"] = ["Cluster:Status"],
            ["Nodes"] = ["Nodes:Nodes"],
            ["VMs"] = ["Vms:Vms"],
            ["Containers"] = ["Vms:Vms"],
            ["Storages"] = ["Storages:Storages"],
            ["RRD Data"] = ["Cluster:RRD Data"],
            ["Logs"] = ["Cluster:Logs"],
            ["Package Versions"] = ["Cluster:Package Version"],
            ["Apt Update"] = ["Cluster:Apt Update"],
            ["Network"] = ["Cluster:Network"],
            ["Disks"] = ["Cluster:Disks"],
            ["Snapshots Usage"] = ["Cluster:Snapshots Usage"],
        };

        foreach (var (keyword, tableKeys) in keywordToTable)
        {
            if (sectionText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                foreach (var tableKey in tableKeys)
                {
                    if (_tablePositions.TryGetValue(tableKey, out var position))
                    {
                        result.Add(position);
                    }
                }
            }
        }

        return result;
    }

    //private void AddIso27001Sections(IXLWorksheet ws, ref int row)
    //=> AddComplianceMappingSection(ws, ref row, "ISO/IEC 27001 Annex A – Technical Controls Mapping",
    //[
    //    new("A.8",  "Asset Management", "See: Nodes, VMs, Containers, Storages sheets"),
    //    new("A.9",  "Access Control", "See: Users, Roles, ACL, API Tokens, TFA tables"),
    //    new("A.10", "Cryptography", "See: Certificates, Backup Encryption, Storage Encryption tables"),
    //    new("A.12", "Operations Security", "See: Services Status, Backup Jobs, Logs, Package Versions tables"),
    //    new("A.13", "Communications Security", "See: Firewall Rules (Cluster, Node, VM/Container), Network Configuration, SDN tables"),
    //    new("A.14", "System Acquisition, Development and Maintenance", "See: Proxmox Version, Kernel Version, Package Update tables"),
    //    new("A.16", "Information Security Incident Management", "See: Logs, Alerts, Failed Login Attempts tables"),
    //    new("A.17", "Information Security Aspects of Business Continuity", "See: Backup, Replication, HA Configuration tables"),
    //    new("A.18", "Compliance", "See: Audit Logs, Configuration Snapshots, Report Metadata")
    //]);

    //private void AddSoc2Sections(IXLWorksheet ws, ref int row)
    //=> AddComplianceMappingSection(ws, ref row, "SOC 2 – Trust Service Criteria Mapping",
    //[
    //    new("CC6.1", "Logical Access Controls", "See: Users, Roles, ACL, API Tokens, TFA tables"),
    //    new("CC6.2", "User Authentication", "See: Password Policy, TFA Status tables"),
    //    new("CC6.6", "System Operations", "See: Services Status, Cluster Health, HA tables"),
    //    new("CC6.7", "Change Management", "See: Proxmox Version History, Package Versions, Apt Update tables"),
    //    new("CC7.1", "System Monitoring", "See: Logs, Alerts, Failed Authentication tables"),
    //    new("CC7.2", "Anomaly Detection", "See: RRD Data, Resource Utilization tables"),
    //    new("CC8.1", "Availability", "See: HA Configuration, Backup, Replication tables")
    //]);

    //private void AddGdprSections(IXLWorksheet ws, ref int row)
    //=> AddComplianceMappingSection(ws, ref row, "GDPR – Technical Safeguards Mapping",
    //[
    //    new("Art. 30", "Records of Processing Activities", "See: Nodes, VMs, Containers sheets for system inventory"),
    //    new("Art. 32", "Security of Processing", "See: Firewall Rules, TFA, Certificates, Encryption tables"),
    //    new("Art. 25", "Data Protection by Design and by Default", "See: Network Segmentation, Access Control tables"),
    //    new("Art. 33–34", "Personal Data Breach Management", "See: Backup, Replication, Logs tables"),
    //    new("Art. 5(1)(f)", "Integrity and Confidentiality", "See: Access Control, Logging, Encryption tables")
    //]);

    //private void AddPciDssSections(IXLWorksheet ws, ref int row)
    //    => AddComplianceMappingSection(ws, ref row, "PCI-DSS – Infrastructure Control Mapping",
    //    [
    //        new("Req. 1", "Network Security Controls", Section: "See: Firewall Rules, Network Segmentation tables"),
    //        new("Req. 2",  "Secure Configuration", "See: Services Status, Default Settings, Hardening tables"),
    //        new("Req. 7",  "Restrict Access to System Components", "See: Users, Roles, ACL tables"),
    //        new("Req. 8",  "Identify and Authenticate Access", "See: TFA Status, API Tokens tables"),
    //        new("Req. 10", "Log and Monitor Access", "See: Logs, RRD Data, Audit tables"),
    //        new("Req. 12", "Security Monitoring Support", "See: Alerts, Incident Evidence tables")
    //    ]);

    #endregion
}
