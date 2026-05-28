/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Exporters.Excel;
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;
using Microsoft.Extensions.Localization;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MigraDocColors = MigraDoc.DocumentObjectModel.Colors;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Services;

public class UpdaterService(IStringLocalizer<UpdaterService> L, ISettingsService settingsService) : IUpdaterService
{
    protected IStringLocalizer L { get; } = L;
    protected ISettingsService SettingsService { get; } = settingsService;

    public Stream GenerateReport(string clusterName, IEnumerable<ClusterResourceUpdateScanInfo> items, ReportFormat format)
        => format switch
        {
            ReportFormat.Pdf => GeneratePdf(clusterName, items),
            ReportFormat.Excel => GenerateExcel(clusterName, items),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

    protected virtual MemoryStream GeneratePdf(string clusterName, IEnumerable<ClusterResourceUpdateScanInfo> items)
    {
        var document = new Document();
        var section = CreateSection(document);

        AddTitle(section, clusterName, items);
        AddUpdateResultsTable(section, items);
        AddErrorsSection(section, items);
        AddFooter(section);

        return Render(document);
    }

    protected void AddErrorsSection(Section section, IEnumerable<ClusterResourceUpdateScanInfo> items)
    {
        var errors = items.Where(a => a.UpdateScanStatus == UpdateInfoStatus.InError && !string.IsNullOrEmpty(a.Error))
                          .OrderBy(a => a.Node)
                          .ThenBy(a => a.Name)
                          .ToList();

        if (errors.Count == 0) { return; }

        section.AddPageBreak();

        var heading = section.AddParagraph(L["Errors ({0})", errors.Count]);
        heading.Format.Font.Size = 14;
        heading.Format.Font.Bold = true;
        heading.Format.Font.Color = MigraDocColors.Red;
        heading.Format.SpaceAfter = Unit.FromMillimeter(5);

        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.Format.Font.Size = 9;

        var totalAvailableWidth = Unit.FromCentimeter(19);
        var totalParts = 18 + 27 + 55;
        table.AddColumn(totalAvailableWidth * 18 / totalParts);
        table.AddColumn(totalAvailableWidth * 27 / totalParts);
        table.AddColumn(totalAvailableWidth * 55 / totalParts);

        var headerRow = table.AddRow();
        headerRow.Shading.Color = MigraDocColors.LightGray;
        headerRow.Format.Font.Bold = true;
        headerRow.Cells[0].AddParagraph(L["Node"]);
        headerRow.Cells[1].AddParagraph(L["VM / CT"]);
        headerRow.Cells[2].AddParagraph(L["Error"]);

        foreach (var item in errors)
        {
            var row = table.AddRow();
            AddBreakableText(row.Cells[0], item.Node);
            AddBreakableText(row.Cells[1], $"{item.VmType} {item.VmId} - {item.Name}");
            AddBreakableText(row.Cells[2], item.Error);
        }
    }

    protected virtual Stream GenerateExcel(string clusterName, IEnumerable<ClusterResourceUpdateScanInfo> items)
    {
        var builder = new ExcelBuilder(SettingsService.GetAppSettings());

        AddUpdatesSheet(builder, items);
        AddErrorsSheet(builder, items);

        return builder.Build(L["Update result of cluster '{0}' Date {1}", clusterName, items.Min(a => a.UpdateScanTimestamp)!]);
    }

    protected void AddErrorsSheet(ExcelBuilder builder, IEnumerable<ClusterResourceUpdateScanInfo> items)
    {
        var errors = items.Where(a => a.UpdateScanStatus == UpdateInfoStatus.InError && !string.IsNullOrEmpty(a.Error))
                          .OrderBy(a => a.Node)
                          .ThenBy(a => a.Name)
                          .Select(a => new
                          {
                              a.Node,
                              VmType = a.VmType.ToString(),
                              a.VmId,
                              a.Name,
                              a.Error,
                              Timestamp = a.UpdateScanTimestamp,
                          })
                          .ToList();

        if (errors.Count == 0) { return; }

        builder.AddSheet("Errors", L["{0} VM/CT failed the update scan", errors.Count], errors);
    }

    protected void AddUpdatesSheet(ExcelBuilder builder, IEnumerable<ClusterResourceUpdateScanInfo> items)
    {
        var rows = items.OrderBy(a => a.Node)
                        .ThenBy(a => a.Description)
                        .Select(a => new
                        {
                            a.Node,
                            a.Description,
                            Status = a.UpdateScanStatus.ToString(),
                            Normal = a.UpdateNormalAvailable,
                            Security = a.UpdateSecurityAvailable,
                            Reboot = a.UpdateRequireReboot,
                            Timestamp = a.UpdateScanTimestamp,
                            a.Error,
                        });

        builder.AddSheet("Updates", L["VM/CT update scan results"], rows);
    }

    protected static Section CreateSection(Document document)
    {
        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromMillimeter(10);
        section.PageSetup.BottomMargin = Unit.FromMillimeter(20);
        section.PageSetup.LeftMargin = Unit.FromMillimeter(10);
        section.PageSetup.RightMargin = Unit.FromMillimeter(10);
        return section;
    }

    protected void AddTitle(Section section, string clusterName, IEnumerable<ClusterResourceUpdateScanInfo> items)
    {
        var title = section.AddParagraph();
        title.AddFormattedText(L["Update result of cluster '{0}' Date {1}", clusterName, items.Min(a => a.UpdateScanTimestamp)!], TextFormat.Bold);
        title.Format.Font.Size = 14;
        title.Format.SpaceAfter = Unit.FromMillimeter(5);
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

    private void AddUpdateResultsTable(Section section, IEnumerable<ClusterResourceUpdateScanInfo> items)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.Format.Font.Size = 9;

        var totalAvailableWidth = Unit.FromCentimeter(19);

        var totalParts = 18 + 45 + 12 + 8 + 8 + 9;
        table.AddColumn(totalAvailableWidth * 18 / totalParts);
        table.AddColumn(totalAvailableWidth * 45 / totalParts);
        table.AddColumn(totalAvailableWidth * 12 / totalParts);
        table.AddColumn(totalAvailableWidth * 8 / totalParts);
        table.AddColumn(totalAvailableWidth * 8 / totalParts);
        table.AddColumn(totalAvailableWidth * 9 / totalParts);

        var headerRow = table.AddRow();
        headerRow.Shading.Color = MigraDocColors.LightGray;
        headerRow.Format.Font.Bold = true;

        headerRow.Cells[0].AddParagraph(L["Node"]);
        headerRow.Cells[1].AddParagraph(L["Description"]);
        headerRow.Cells[2].AddParagraph(L["Status"]);

        var normalPara = headerRow.Cells[3].AddParagraph(L["Normal"]);
        normalPara.Format.Alignment = ParagraphAlignment.Center;
        var securityPara = headerRow.Cells[4].AddParagraph(L["Security"]);
        securityPara.Format.Alignment = ParagraphAlignment.Center;
        var rebootPara = headerRow.Cells[5].AddParagraph(L["Reboot"]);
        rebootPara.Format.Alignment = ParagraphAlignment.Center;

        foreach (var item in items)
        {
            var row = table.AddRow();

            AddBreakableText(row.Cells[0], item.Node);
            AddBreakableText(row.Cells[1], item.Description);

            var statusPara = row.Cells[2].AddParagraph(item.UpdateScanStatus.ToString());
            statusPara.Format.Font.Color = item.UpdateScanStatus switch
            {
                UpdateInfoStatus.InScan or UpdateInfoStatus.Ok => MigraDocColors.Black,
                UpdateInfoStatus.InError => MigraDocColors.Red,
                _ => MigraDocColors.Black
            };

            var normalCellPara = row.Cells[3].AddParagraph(item.UpdateNormalAvailable ? "X" : string.Empty);
            normalCellPara.Format.Alignment = ParagraphAlignment.Center;

            var securityCellPara = row.Cells[4].AddParagraph(item.UpdateSecurityAvailable ? "X" : string.Empty);
            securityCellPara.Format.Alignment = ParagraphAlignment.Center;

            var rebootCellPara = row.Cells[5].AddParagraph(item.UpdateRequireReboot ? "X" : string.Empty);
            rebootCellPara.Format.Alignment = ParagraphAlignment.Center;
        }
    }

    // MigraDoc breaks paragraphs only at whitespace. Long tokens like host names
    // (e.g. "proxmox-node-frankfurt-01.dc.acme.com") or package descriptions with
    // no spaces overflow the cell. Insert zero-width spaces after every separator
    // so the renderer can wrap inside the cell.
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
