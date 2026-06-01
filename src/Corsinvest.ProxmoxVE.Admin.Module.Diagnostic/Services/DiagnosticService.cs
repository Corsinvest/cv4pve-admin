/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Exporters.Excel;
using Corsinvest.ProxmoxVE.Admin.Core.Helpers;
using Corsinvest.ProxmoxVE.Diagnostic.Api;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MigraDocColors = MigraDoc.DocumentObjectModel.Colors;

namespace Corsinvest.ProxmoxVE.Admin.Module.Diagnostic.Services;

public class DiagnosticService(IStringLocalizer<DiagnosticService> L, ISettingsService settingsService) : IDiagnosticService
{
    protected IStringLocalizer L { get; } = L;
    protected ISettingsService SettingsService { get; } = settingsService;

    public virtual string GetHelpUrl(JobDetail jobDetail)
    {
        var url = string.Empty;
        switch (jobDetail.Context)
        {
            case DiagnosticResultContext.Node:
                switch (jobDetail.SubContext)
                {
                    case "EOL": url = "https://pve.proxmox.com/wiki/FAQ"; break;
                    default: break;
                }
                break;

            case DiagnosticResultContext.Cluster: break;
            case DiagnosticResultContext.Storage: break;

            case DiagnosticResultContext.Qemu:
                switch (jobDetail.SubContext)
                {
                    case "Agent": url = "https://pve.proxmox.com/wiki/Qemu-guest-agent"; break;

                    case "OSNotMaintained":
                        if (jobDetail.Description.Contains("Windows"))
                        {
                            url = "https://endoflife.date/windows";
                        }
                        else if (jobDetail.Description.Contains("Linux"))
                        {
                            url = "https://endoflife.date/linux";
                        }
                        break;

                    case "VirtIO":
                        if (jobDetail.Description.Contains("network"))
                        {
                            url = "https://pve.proxmox.com/pve-docs/chapter-qm.html#qm_network_device";
                        }
                        break;

                    case "StartOnBoot":
                        url = "https://pve.proxmox.com/pve-docs/pve-admin-guide.html#qm_startup_and_shutdown";
                        break;

                    default: break;
                }
                break;

            case DiagnosticResultContext.Lxc: break;

            default: break;
        }

        return url;
    }

    public string GetPveResourceUrl(string idResource, DiagnosticResultContext context, string clusterName)
    {
        if (string.IsNullOrEmpty(idResource)) { return "#"; }

        var data = idResource.Split("/");
        return context switch
        {
            DiagnosticResultContext.Node when data.Length > 1 => UrlHelper.Resources.NodeUrl(data[1], clusterName),
            DiagnosticResultContext.Qemu or DiagnosticResultContext.Lxc when data.Length > 3 && long.TryParse(data[3], out var vmid) => UrlHelper.Resources.VmUrl(vmid, clusterName),
            _ => "#",
        };
    }

    public Stream GenerateReport(JobResult result, ReportFormat format)
        => format switch
        {
            ReportFormat.Pdf => GeneratePdf(result),
            ReportFormat.Excel => GenerateExcel(result),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

    protected virtual MemoryStream GeneratePdf(JobResult result)
    {
        var document = new Document();
        var section = CreateSection(document);

        AddTitle(section, result);
        AddIssuesSection(section, result);
        AddFooter(section);

        return Render(document);
    }

    protected virtual Stream GenerateExcel(JobResult result)
    {
        var builder = new ExcelBuilder(SettingsService.GetAppSettings());

        AddIssuesSheet(builder, result);
        AddIgnoredIssuesSheet(builder, result);

        return builder.Build(L["Diagnostic result of cluster '{0}' Date {1}", result.ClusterName, result.Start]);
    }

    protected void AddIssuesSheet(ExcelBuilder builder, JobResult result)
        => builder.AddSheet("Issues",
                            L["Active diagnostic issues"],
                            ToExcelRows(result.Details.Where(a => !a.IsIgnoredIssue)));

    protected void AddIgnoredIssuesSheet(ExcelBuilder builder, JobResult result)
        => builder.AddSheet("Ignored Issues",
                            L["Issues hidden from the active result"],
                            ToExcelRows(result.Details.Where(a => a.IsIgnoredIssue)));

    private static IEnumerable<object> ToExcelRows(IEnumerable<JobDetail> details)
        => details.OrderBy(a => a.Gravity)
                  .ThenBy(a => a.IdResource)
                  .Select(a => new
                  {
                      Id = a.IdResource,
                      a.ErrorCode,
                      Context = a.Context.ToString(),
                      SubContext = a.SubContext ?? string.Empty,
                      a.Description,
                      Gravity = a.Gravity.ToString(),
                  });

    protected Section CreateSection(Document document)
    {
        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromMillimeter(10);
        section.PageSetup.BottomMargin = Unit.FromMillimeter(20);
        section.PageSetup.LeftMargin = Unit.FromMillimeter(10);
        section.PageSetup.RightMargin = Unit.FromMillimeter(10);
        return section;
    }

    protected void AddTitle(Section section, JobResult result)
    {
        var title = section.AddParagraph();
        title.AddFormattedText(L["Diagnostic result of cluster '{0}' Date {1}", result.ClusterName, result.Start], TextFormat.Bold);
        title.Format.Font.Size = 14;
        title.Format.SpaceAfter = Unit.FromMillimeter(5);
    }

    protected void AddIssuesSection(Section section, JobResult result)
    {
        AddResultsTable(section, result.Details.Where(a => !a.IsIgnoredIssue));

        var ignoredIssues = result.Details.Where(a => a.IsIgnoredIssue).ToList();
        if (ignoredIssues.Count != 0)
        {
            section.AddPageBreak();
            var ignoredTitle = section.AddParagraph(L["Ignored"]);
            ignoredTitle.Format.Font.Size = 16;
            ignoredTitle.Format.Font.Bold = true;
            ignoredTitle.Format.SpaceAfter = Unit.FromMillimeter(5);
            AddResultsTable(section, ignoredIssues);
        }
    }

    protected void AddFooter(Section section)
    {
        var appSettings = SettingsService.GetAppSettings();

        var footer = section.Footers.Primary.AddTable();
        footer.Borders.Visible = false;

        var totalAvailableWidth = Unit.FromCentimeter(19);

        footer.AddColumn(totalAvailableWidth / 2);
        footer.AddColumn(totalAvailableWidth / 2);

        var row = footer.AddRow();
        row.TopPadding = 3;

        var leftCell = row.Cells[0];
        leftCell.Format.Alignment = ParagraphAlignment.Left;
        var datePara = leftCell.AddParagraph();
        datePara.AddText(DateTime.Now.ToString("dd/MM/yyyy") + " - " + L["Page"] + " ");
        datePara.AddPageField();
        datePara.AddText(" " + L["of"] + " ");
        datePara.AddNumPagesField();
        datePara.Format.Font.Size = 10;

        var rightCell = row.Cells[1];
        rightCell.Format.Alignment = ParagraphAlignment.Right;

        var poweredPara = rightCell.AddParagraph();
        poweredPara.Format.Alignment = ParagraphAlignment.Right;
        poweredPara.AddText("Powered by ");
        poweredPara.Format.Font.Size = 10;

        var hyperlink = poweredPara.AddHyperlink(appSettings.AppUrl, HyperlinkType.Web);
        hyperlink.AddText(appSettings.AppName);
        hyperlink.Font.Underline = Underline.Single;
        hyperlink.Font.Color = MigraDocColors.Blue;
        hyperlink.Font.Size = 10;
    }

    protected static MemoryStream Render(Document document)
    {
        var pdfRenderer = new PdfDocumentRenderer { Document = document };
        pdfRenderer.RenderDocument();
        var ms = new MemoryStream();
        pdfRenderer.PdfDocument.Save(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    private void AddResultsTable(Section section, IEnumerable<JobDetail> results)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.Format.Font.Size = 9;

        var totalAvailableWidth = Unit.FromCentimeter(19);

        // Wider Id column so long IDs like "access/user/cv4pve-admin@pve" wrap inside the cell
        // instead of overflowing onto the next column.
        table.AddColumn(totalAvailableWidth * 0.22);
        table.AddColumn(totalAvailableWidth * 0.13);
        table.AddColumn(totalAvailableWidth * 0.15);
        table.AddColumn(totalAvailableWidth * 0.42);
        table.AddColumn(totalAvailableWidth * 0.08);

        var headerRow = table.AddRow();
        headerRow.Shading.Color = MigraDocColors.LightGray;
        headerRow.Cells[0].AddParagraph(L["Id"]);
        headerRow.Cells[1].AddParagraph(L["Context"]);
        headerRow.Cells[2].AddParagraph(L["SubContext"]);
        headerRow.Cells[3].AddParagraph(L["Description"]);
        headerRow.Cells[4].AddParagraph(L["Gravity"]);
        headerRow.Format.Font.Bold = true;

        foreach (var item in results.OrderBy(a => a.Gravity).ThenBy(a => a.IdResource))
        {
            var row = table.AddRow();
            AddBreakableText(row.Cells[0], item.IdResource);
            row.Cells[1].AddParagraph(item.Context.ToString());
            row.Cells[2].AddParagraph(item.SubContext ?? string.Empty);
            row.Cells[3].AddParagraph(item.Description ?? string.Empty);
            var cell = row.Cells[4];
            cell.AddParagraph(item.Gravity.ToString());
            ApplyGravityShading(cell, item.Gravity);
        }
    }

    /// <summary>
    /// Pastel background + dark foreground for gravity cells in PDF tables.
    /// Same palette as the Excel conditional formatting — keeps the two reports visually consistent.
    /// </summary>
    protected static void ApplyGravityShading(Cell cell, DiagnosticResultGravity gravity)
    {
        var (bg, fg) = GravityPalette(gravity);
        cell.Shading.Color = Color.FromRgb(bg.R, bg.G, bg.B);
        cell.Format.Font.Color = Color.FromRgb(fg.R, fg.G, fg.B);
    }

    /// <summary>
    /// Pastel palette shared between PDF and Excel for gravity/status cells.
    /// Returns (background, foreground) as standard <see cref="System.Drawing.Color"/>.
    /// </summary>
    protected static (System.Drawing.Color Background, System.Drawing.Color Foreground) GravityPalette(DiagnosticResultGravity gravity) => gravity switch
    {
        DiagnosticResultGravity.Critical => (System.Drawing.Color.FromArgb(0xF8, 0xD7, 0xDA), System.Drawing.Color.FromArgb(0x72, 0x1C, 0x24)),
        DiagnosticResultGravity.Warning => (System.Drawing.Color.FromArgb(0xFF, 0xF3, 0xCD), System.Drawing.Color.FromArgb(0x85, 0x64, 0x04)),
        DiagnosticResultGravity.Info => (System.Drawing.Color.FromArgb(0xD1, 0xEC, 0xF1), System.Drawing.Color.FromArgb(0x0C, 0x54, 0x60)),
        DiagnosticResultGravity.Ok => (System.Drawing.Color.FromArgb(0xD4, 0xED, 0xDA), System.Drawing.Color.FromArgb(0x15, 0x57, 0x24)),
        _ => (System.Drawing.Color.White, System.Drawing.Color.Black),
    };

    // MigraDoc breaks paragraphs only at whitespace. Long tokens like
    // "access/user/cv4pve-admin@pve" have no spaces and overflow the cell.
    // Insert zero-width spaces after every separator so the renderer can wrap.
    private static void AddBreakableText(Cell cell, string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            cell.AddParagraph(string.Empty);
            return;
        }

        const char zws = '​';
        var withBreaks = new System.Text.StringBuilder(text.Length * 2);
        foreach (var ch in text)
        {
            withBreaks.Append(ch);
            if (ch is '/' or '\\' or '.' or '@' or '-' or '_' or ':')
            {
                withBreaks.Append(zws);
            }
        }
        cell.AddParagraph(withBreaks.ToString());
    }
}
