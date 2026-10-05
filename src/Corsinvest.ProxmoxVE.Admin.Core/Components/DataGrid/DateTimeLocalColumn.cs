/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;

public class DateTimeLocalColumn<TItem> : RadzenDataGridColumn<TItem> where TItem : notnull
{
    protected override void OnInitialized()
    {
        base.OnInitialized();

        // The grid export reads the stored value, which is UTC: it must write the local time the grid shows.
        ExportValue ??= GetValue;
    }

    public override object? GetValue(TItem item)
    {
        var value = PropertyAccess.GetValue(item, Property);
        return value is DateTime dt
                ? dt.ToLocalTime()
                : value;
    }
}
