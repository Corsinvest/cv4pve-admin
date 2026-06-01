/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using ClosedXML.Excel;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDocColors = MigraDoc.DocumentObjectModel.Colors;

namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class SubscriptionGateReportHelper
{
    public static void AddEnterprisePlaceholder(Section section,
                                                string availableLabel,
                                                string ctaLabel)
    {
        // Centered yellow-bordered box that mimics SubscriptionGate.razor.
        var table = section.AddTable();
        table.Borders.Width = 1.5;
        table.Borders.Color = MigraDocColors.Orange;
        table.AddColumn(Unit.FromCentimeter(19));

        var row = table.AddRow();
        var cell = row.Cells[0];
        cell.Shading.Color = new Color(255, 248, 220); // soft yellow background
        cell.Format.Alignment = ParagraphAlignment.Center;
        cell.VerticalAlignment = VerticalAlignment.Center;
        cell.Format.SpaceBefore = Unit.FromMillimeter(4);
        cell.Format.SpaceAfter = Unit.FromMillimeter(4);

        var msg = cell.AddParagraph(availableLabel);
        msg.Format.Font.Size = 12;
        msg.Format.Font.Bold = true;
        msg.Format.Alignment = ParagraphAlignment.Center;

        var link = cell.AddParagraph();
        link.Format.Alignment = ParagraphAlignment.Center;
        link.Format.SpaceBefore = Unit.FromMillimeter(2);
        var hyperlink = link.AddHyperlink(ApplicationHelper.ShopSubscriptionUrl, HyperlinkType.Url);
        var text = hyperlink.AddFormattedText(ctaLabel, TextFormat.Underline);
        text.Color = MigraDocColors.Blue;
    }

    public static int AddEnterprisePlaceholder(IXLWorksheet sheet,
                                               int startRow,
                                               string availableLabel,
                                               string ctaLabel)
    {
        var titleCell = sheet.Cell(startRow, 1);
        titleCell.Value = availableLabel;
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 14;
        titleCell.Style.Font.FontColor = XLColor.DarkOrange;
        titleCell.Style.Fill.BackgroundColor = XLColor.FromArgb(255, 248, 220);
        sheet.Range(startRow, 1, startRow, 6).Merge();

        var linkCell = sheet.Cell(startRow + 1, 1);
        linkCell.Value = ctaLabel;
        linkCell.SetHyperlink(new XLHyperlink(ApplicationHelper.ShopSubscriptionUrl));
        linkCell.Style.Font.Underline = XLFontUnderlineValues.Single;
        linkCell.Style.Font.FontColor = XLColor.Blue;
        sheet.Range(startRow + 1, 1, startRow + 1, 6).Merge();

        return 2;
    }
}
