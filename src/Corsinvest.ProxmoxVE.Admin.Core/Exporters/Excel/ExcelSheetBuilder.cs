/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using ClosedXML.Excel;

namespace Corsinvest.ProxmoxVE.Admin.Core.Exporters.Excel;

/// <summary>
/// Builder for a single Excel sheet that can contain multiple tables.
/// </summary>
public class ExcelSheetBuilder
{
    private readonly IXLWorksheet _ws;
    private int _currentRow = 1;

    internal ExcelSheetBuilder(IXLWorksheet ws) => _ws = ws;

    /// <summary>
    /// Writes a bold heading followed by an italic gray text block. Use for disclaimers
    /// or short notes that should appear above the data tables on the sheet.
    /// </summary>
    public ExcelSheetBuilder AddNote(string heading, string body)
    {
        _ws.Cell(_currentRow, 1).Value = heading;
        _ws.Cell(_currentRow, 1).Style.Font.SetBold(true);
        _ws.Cell(_currentRow, 1).Style.Font.SetFontSize(11);
        _currentRow++;

        _ws.Cell(_currentRow, 1).Value = body;
        _ws.Cell(_currentRow, 1).Style.Font.SetItalic(true);
        _ws.Cell(_currentRow, 1).Style.Font.SetFontColor(XLColor.Gray);
        _ws.Cell(_currentRow, 1).Style.Alignment.SetWrapText(true);
        _ws.Range(_currentRow, 1, _currentRow, 6).Merge();
        _ws.Row(_currentRow).Height = 60;
        _currentRow += 2;
        return this;
    }

    public ExcelSheetBuilder AddTable<T>(string title, IEnumerable<T> data)
    {
        // Section title
        _ws.Cell(_currentRow, 1).Value = title;
        _ws.Cell(_currentRow, 1).Style.Font.SetBold(true);
        _ws.Cell(_currentRow, 1).Style.Font.SetFontSize(13);
        _currentRow++;

        var table = _ws.Cell(_currentRow, 1).InsertTable(data, true);
        table.Theme = XLTableTheme.TableStyleMedium2;
        table.ShowAutoFilter = true;

        // Pretty-print header names (PascalCase → "Pascal Case") and
        // auto-apply conditional formatting on well-known gravity/status columns.
        foreach (var field in table.Fields)
        {
            var rawName = field.HeaderCell.Value.ToString();
            field.HeaderCell.Value = ToHeaderName(rawName);

            if (IsGravityColumn(rawName))
            {
                ApplyGravityConditionalFormatting(table.DataRange.Column(field.Index + 1));
            }
        }

        _currentRow += table.RowCount() + 2; // header + data rows + gap
        return this;
    }

    private static bool IsGravityColumn(string name)
        => name is "Gravity" or "Status";

    private static void ApplyGravityConditionalFormatting(IXLRangeColumn dataCol)
    {
        Apply("Critical", "#F8D7DA", "#721C24");
        Apply("Fail", "#F8D7DA", "#721C24");
        Apply("Warning", "#FFF3CD", "#856404");
        Apply("Info", "#D1ECF1", "#0C5460");
        Apply("Ok", "#D4EDDA", "#155724");
        Apply("Pass", "#D4EDDA", "#155724");

        void Apply(string value, string bg, string fg)
        {
            var cf = dataCol.AddConditionalFormat().WhenEquals(value);
            cf.Fill.SetBackgroundColor(XLColor.FromHtml(bg));
            cf.Font.SetFontColor(XLColor.FromHtml(fg));
            cf.Font.SetBold(true);
        }
    }

    public ExcelSheetBuilder AddEnterprisePlaceholder(string availableLabel, string ctaLabel)
    {
        _currentRow += SubscriptionGateReportHelper.AddEnterprisePlaceholder(_ws, _currentRow, availableLabel, ctaLabel);
        return this;
    }

    internal void FinalizeSheet() => _ws.Columns().AdjustToContents(1, 80);

    private static string ToHeaderName(string propertyName)
    {
        // PascalCase → "Pascal Case"
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < propertyName.Length; i++)
        {
            if (i > 0 && char.IsUpper(propertyName[i]) && !char.IsUpper(propertyName[i - 1]))
            {
                sb.Append(' ');
            }
            sb.Append(propertyName[i]);
        }
        return sb.ToString();
    }
}
