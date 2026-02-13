/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;

public class DateTimeLocalColumn<TItem> : RadzenDataGridColumn<TItem> where TItem : notnull
{
    public override object? GetValue(TItem item)
    {
        var value = PropertyAccess.GetValue(item, Property);
        return value is DateTime dt
                ? dt.ToLocalTime()
                : value;
    }
}
