/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;

public class SwitchColumn<TItem> : RadzenDataGridColumn<TItem> where TItem : notnull
{
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Template = item => builder =>
        {
            builder.OpenComponent<RadzenSwitch>(0);
            builder.AddAttribute(1, nameof(RadzenSwitch.Value), Convert.ToBoolean(GetValue(item)));
            builder.AddAttribute(2, nameof(RadzenSwitch.Disabled), true);
            builder.CloseComponent();
        };
    }
}
