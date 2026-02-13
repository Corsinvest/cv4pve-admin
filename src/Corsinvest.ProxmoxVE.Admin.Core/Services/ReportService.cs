/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using MigraDocColors = MigraDoc.DocumentObjectModel.Colors;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

internal class ReportService(IStringLocalizer<ReportService> L, ISettingsService settingsService) : IReportService
{
    public MemoryStream GeneratePdf(string title)
    {
        var appSettings = settingsService.GetAppSettings();

        var document = new Document();
        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromMillimeter(10);
        section.PageSetup.BottomMargin = Unit.FromMillimeter(20);
        section.PageSetup.LeftMargin = Unit.FromMillimeter(10);
        section.PageSetup.RightMargin = Unit.FromMillimeter(10);

        var titlePh = section.AddParagraph();
        titlePh.AddFormattedText(title, TextFormat.Bold);
        titlePh.Format.Font.Size = 14;
        titlePh.Format.SpaceAfter = Unit.FromMillimeter(5);

        var subscriptionParagraph = section.AddParagraph();
        subscriptionParagraph.Format.SpaceAfter = Unit.FromMillimeter(3);

        // Diamond icon
        var diamond = subscriptionParagraph.AddFormattedText("◆ ", TextFormat.NotBold);
        diamond.Font.Color = MigraDocColors.Orange;
        diamond.Font.Size = 14;

        // "Requires subscription: " text
        subscriptionParagraph.AddText(L["Requires subscription"] + ": ");
        subscriptionParagraph.Format.Font.Size = 12;

        // "Enterprise" in bold
        subscriptionParagraph.AddFormattedText(L["Enterprise"], TextFormat.Bold);

        // Spacing before link
        subscriptionParagraph.Format.SpaceAfter = Unit.FromMillimeter(2);

        // "Get Enterprise" link
        var linkParagraph = section.AddParagraph();
        var hyperlinkSubscription = linkParagraph.AddHyperlink(ApplicationHelper.UrlShopSubscription, HyperlinkType.Web);
        hyperlinkSubscription.AddFormattedText(L["Get Enterprise"], TextFormat.Bold);
        hyperlinkSubscription.Font.Underline = Underline.Single;
        hyperlinkSubscription.Font.Color = MigraDocColors.Blue;
        hyperlinkSubscription.Font.Size = 12;

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
        datePara.AddText(DateTime.Now.ToString("dd/MM/yyyy") + " - Pagina ");
        datePara.AddPageField();
        datePara.AddText(" di ");
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

        var pdfRenderer = new PdfDocumentRenderer { Document = document };
        pdfRenderer.RenderDocument();
        var ms = new MemoryStream();
        pdfRenderer.PdfDocument.Save(ms, false);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
}
