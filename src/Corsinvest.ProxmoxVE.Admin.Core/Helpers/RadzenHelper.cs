/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Helpers;

public static class RadzenHelper
{
    public static void MakeDataGridSettings(DataGridSettings dataGridSettings, IEnumerable<string> propertyNames)
        => dataGridSettings.Columns = [.. propertyNames.Select((a, idx) => new DataGridColumnSettings
            {
                Property = a,
                Visible = true,
                OrderIndex = idx
            })];

    public static DataGridSettings MakeDataGridSettings(IEnumerable<string> propertyNames)
        => new()
        {
            Columns = [.. propertyNames.Select((a, idx) => new DataGridColumnSettings
            {
                    Property = a,
                    Visible = true,
                    OrderIndex = idx
            })]
        };
}
