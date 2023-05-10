/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Ardalis.Specification;
using Corsinvest.AppHero.Core.Notification;
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Diagnostic.Repository;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Diagnostic.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Corsinvest.ProxmoxVE.Admin.Diagnostic;

internal class Helper
{
    public static async Task Delete(IServiceScope scope, IEnumerable<int> ids)
    {
        var executionsRepo = scope.GetRepository<Execution>();
        foreach (var item in ids)
        {
            await executionsRepo.DeleteAsync((await executionsRepo.GetByIdAsync(item))!);
        }
    }

    private static ModuleClusterOptions GetModuleClusterOptions(IServiceScope scope, string clusterName)
        => scope.GetModuleClusterOptions<Options, ModuleClusterOptions>(clusterName);

    private static async Task<List<DiagnosticResult>> GetIgnoredIssue(IServiceScope scope, string clusterName)
        => await GetIgnoredIssue(scope.GetReadRepository<IgnoredIssue>(), clusterName);

    public static async Task<List<DiagnosticResult>> GetIgnoredIssue(IReadRepositoryBase<IgnoredIssue> ignoredIssuesRepo, string clusterName)
        => (await ignoredIssuesRepo.ListAsync(new IgnoredIssueSpec(clusterName)))
                        .Select(a => a.ToDiagnosticResult())
                        .ToList();

    public static async Task Rescan(IServiceScope scope, string clusterName)
    {
        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(Helper));
        var moduleClusterOptions = GetModuleClusterOptions(scope, clusterName);
        var executionsRepo = scope.GetRepository<Execution>();

        using (logger.LogTimeOperation(LogLevel.Information, true, "Rescan Cluster {clusterName}", clusterName))
        {
            var ignoredIssues = await GetIgnoredIssue(scope, clusterName);

            foreach (var item in await executionsRepo.ListAsync(new ExecutionSpec(clusterName).Include()))
            {
                SetCounts(item, Analyze(item.Data, moduleClusterOptions, ignoredIssues));
            }

            await executionsRepo.SaveChangesAsync();
        }
    }

    private static void SetCounts(Execution execution, IEnumerable<DiagnosticResult> results)
    {
        int GetData(DiagnosticResultGravity gravity) => results.Count(a => a.Gravity == gravity && !a.IsIgnoredIssue);

        execution.Critical = GetData(DiagnosticResultGravity.Critical);
        execution.Info = GetData(DiagnosticResultGravity.Info);
        execution.Warning = GetData(DiagnosticResultGravity.Warning);
    }

    public static ICollection<DiagnosticResult> Analyze(ExecutionData executionData, ModuleClusterOptions moduleClusterOptions, IEnumerable<DiagnosticResult> ignoreIssues)
        => Application.Analyze(JsonConvert.DeserializeObject<InfoHelper.Info>(executionData.Data), moduleClusterOptions, ignoreIssues.ToList());

    public static async Task Create(IServiceScope scope, string clusterName)
    {
        var loggerFactory = scope.GetLoggerFactory();
        var logger = loggerFactory.CreateLogger(typeof(Helper));
        var moduleClusterOptions = GetModuleClusterOptions(scope, clusterName);

        using (logger.LogTimeOperation(LogLevel.Information, true, "Execute diagnostic cluster {clusterName}", clusterName))
        {
            var tasksDays = 10;

            InfoHelper.Info info;
            using (logger.LogTimeOperation(LogLevel.Information, true, "Collect data from cluster"))
            {
                var client = await scope.GetPveClient(clusterName);
                info = await InfoHelper.Collect(client, true, tasksDays, true, false);
            }

            var execution = new Execution()
            {
                ClusterName = clusterName,
                Date = info.Date,
                Data = new ExecutionData
                {
                    Data = JsonConvert.SerializeObject(info)
                }
            };

            var ignoredIssues = await GetIgnoredIssue(scope, clusterName);
            var ignoredIssuesRepo = scope.GetReadRepository<IgnoredIssue>();

            var results = Analyze(execution.Data, moduleClusterOptions, ignoredIssues);
            SetCounts(execution, results);

            var executionsRepo = scope.GetRepository<Execution>();
            await executionsRepo.AddAsync(execution);

            //delete old
            await executionsRepo.DeleteRangeAsync(await executionsRepo.ListAsync(new ExecutionSpec(clusterName, moduleClusterOptions.Keep)));

            //send notification
            if (moduleClusterOptions.NotificationChannels?.Any() is true)
            {
                var appOptions = scope.GetAppOptions();

                var L = scope.ServiceProvider.GetRequiredService<IStringLocalizer<Helper>>();
                using var ms = GeneratePdf(L, appOptions, execution, results);

                await scope.GetNotificationService().SendAsync(moduleClusterOptions.NotificationChannels, new()
                {
                    Subject = L["{appName} - Diagnostic result", appOptions.Name],
                    Body = L["Diagnostic result of {date}", info.Date],
                    Attachments = new[] { new Attachment(ms, "Diagnostic.pdf", System.Net.Mime.MediaTypeNames.Application.Pdf) }
                });
            }
        }
    }

    public static MemoryStream GeneratePdf(IStringLocalizer L, AppOptions appOptions, Execution execution, ICollection<DiagnosticResult> results)
    {
        var ms = new MemoryStream();
        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(10, Unit.Millimetre);

                page.Content().Column(column =>
                {
                    column.Spacing(5);

                    column.Item().Text(string.Format(L["Diagnostic result from {0} Date {1}"], appOptions.Name, execution.Date))
                                 .FontSize(16)
                                 .Bold();

                    column.Item().Table(table => ContentTable(L, table, results.Where(a => !a.IsIgnoredIssue).ToList()));

                    column.Item().PageBreak();

                    column.Item().Text(L["Ignored"]).FontSize(16).Bold();

                    column.Item().Table(table => ContentTable(L, table, results.Where(a => a.IsIgnoredIssue).ToList()));
                });

                static void ContentTable(IStringLocalizer L, TableDescriptor table, ICollection<DiagnosticResult> results)
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(15);
                        columns.RelativeColumn(20);
                        columns.RelativeColumn(25);
                        columns.RelativeColumn(60);
                        columns.RelativeColumn(15);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text(L["Id"]);
                        header.Cell().Element(CellStyle).Text(L["Context"]);
                        header.Cell().Element(CellStyle).Text(L["SubContext"]);
                        header.Cell().Element(CellStyle).Text(L["Description"]);
                        header.Cell().Element(CellStyle).Text(L["Gravity"]);

                        static IContainer CellStyle(IContainer container)
                            => container.DefaultTextStyle(x => x.SemiBold())
                                            .PaddingVertical(5)
                                            .BorderBottom(1)
                                            .BorderColor(QuestPDF.Helpers.Colors.Black);
                    });

                    foreach (var item in results.OrderBy(a => a.Gravity).ThenBy(a => a.Id))
                    {
                        var color = item.Gravity switch
                        {
                            DiagnosticResultGravity.Info => QuestPDF.Helpers.Colors.Black,
                            DiagnosticResultGravity.Warning => QuestPDF.Helpers.Colors.Orange.Lighten1,
                            DiagnosticResultGravity.Critical => QuestPDF.Helpers.Colors.Red.Lighten1,
                            _ => QuestPDF.Helpers.Colors.Black,
                        };

                        table.Cell().Element(CellStyle).Text(item.Id).FontSize(12);
                        table.Cell().Element(CellStyle).Text(item.Context.ToString()).FontSize(12);
                        table.Cell().Element(CellStyle).Text(item.SubContext).FontSize(12);
                        table.Cell().Element(CellStyle).Text(item.Description).FontSize(12);
                        table.Cell().Element(CellStyle).Text(item.Gravity.ToString()).FontColor(color).FontSize(12);

                        static IContainer CellStyle(IContainer container)
                            => container.BorderBottom(1)
                                        .BorderColor(QuestPDF.Helpers.Colors.Grey.Lighten2)
                                        .PaddingVertical(5);
                    }
                }

                page.Footer().Column(column =>
                {
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(10);
                            columns.RelativeColumn(20);
                            columns.RelativeColumn(5);
                            columns.RelativeColumn(7);
                        });

                        table.Cell().Element(x => x).Text(DateTime.Now.ToString()).FontSize(10);
                        table.Cell().Element(x => x).AlignCenter().Text(x =>
                        {
                            x.CurrentPageNumber().FontSize(10);
                            x.Span(" / ").FontSize(10);
                            x.TotalPages().FontSize(10);
                        });

                        table.Cell().Element(x => x).Text("Powered by ").FontSize(10);
                        table.Cell().Element(x => x).Hyperlink(appOptions.Url).Text(appOptions.Name)
                                    .Underline()
                                    .FontColor(QuestPDF.Helpers.Colors.Blue.Lighten1)
                                    .FontSize(10);
                    });
                });
            });
        })
        .GeneratePdf(ms);

        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}