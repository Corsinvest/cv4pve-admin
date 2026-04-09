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

    public ExcelSheetBuilder AddTable<T>(string title, IEnumerable<T> data)
    {
        // Section title
        _ws.Cell(_currentRow, 1).Value = title;
        _ws.Cell(_currentRow, 1).Style.Font.SetBold(true);
        _ws.Cell(_currentRow, 1).Style.Font.SetFontSize(13);
        _currentRow++;

        var list = data.ToList();
        if (list.Count == 0)
        {
            _ws.Cell(_currentRow, 1).Value = "(no data)";
            _ws.Cell(_currentRow, 1).Style.Font.SetItalic(true);
            _ws.Cell(_currentRow, 1).Style.Font.SetFontColor(XLColor.Gray);
            _currentRow += 2;
            return this;
        }

        var props = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                             .Where(p => p.CanRead && IsSupportedType(p.PropertyType))
                             .ToArray();

        // Header row
        for (var col = 0; col < props.Length; col++)
        {
            var cell = _ws.Cell(_currentRow, col + 1);
            cell.Value = ToHeaderName(props[col].Name);
            cell.Style.Font.SetBold(true);
            cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#4472C4"));
            cell.Style.Font.SetFontColor(XLColor.White);
        }

        var headerRow = _currentRow;
        _currentRow++;

        // Data rows
        foreach (var item in list)
        {
            for (var col = 0; col < props.Length; col++)
            {
                var cell = _ws.Cell(_currentRow, col + 1);
                var value = props[col].GetValue(item);
                SetCellValue(cell, value);
            }
            _currentRow++;
        }

        // Apply table style with autofilter
        var tableRange = _ws.Range(headerRow, 1, _currentRow - 1, props.Length);
        var table = tableRange.CreateTable();
        table.Theme = XLTableTheme.TableStyleMedium2;
        table.ShowAutoFilter = true;

        _currentRow += 2; // gap between tables
        return this;
    }

    internal void FinalizeSheet() => _ws.Columns().AdjustToContents(1, 80);

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null: break;
            case bool b: cell.Value = b; break;
            case int i: cell.Value = i; break;
            case long l: cell.Value = l; break;
            case double d: cell.Value = d; break;
            case float f: cell.Value = f; break;
            case decimal dec: cell.Value = dec; break;
            case DateTime dt: cell.Value = dt; cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss"; break;
            case DateOnly donly: cell.Value = donly.ToDateTime(TimeOnly.MinValue); cell.Style.DateFormat.Format = "yyyy-MM-dd"; break;
            case TimeSpan ts: cell.Value = ts.ToString(); break;
            default: cell.Value = value.ToString(); break;
        }
    }

    private static bool IsSupportedType(Type t)
    {
        var underlying = Nullable.GetUnderlyingType(t) ?? t;
        return underlying.IsPrimitive
            || underlying == typeof(string)
            || underlying == typeof(decimal)
            || underlying == typeof(DateTime)
            || underlying == typeof(DateOnly)
            || underlying == typeof(TimeSpan)
            || underlying.IsEnum;
    }

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
