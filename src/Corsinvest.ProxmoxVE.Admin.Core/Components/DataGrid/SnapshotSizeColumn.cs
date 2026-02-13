/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;

public class SnapshotSizeColumn<TItem> : RadzenDataGridColumn<TItem> where TItem : notnull
{
    [Parameter] public bool ShowWating { get; set; }
    [Parameter] public bool Replication { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        FormatString = FormatHelper.DataFormatBytes;
        FormatProvider = FormatterHelper.FormatProvider;
        Width = "100px";

        Template = item => builder =>
        {
            if (ShowWating)
            {
                builder.OpenComponent<RadzenProgressBarCircular>(0);
                builder.AddAttribute(1, nameof(RadzenProgressBarCircular.ProgressBarStyle), ProgressBarStyle.Primary);
                builder.AddAttribute(2, nameof(RadzenProgressBarCircular.Value), 100d);
                builder.AddAttribute(3, nameof(RadzenProgressBarCircular.ShowValue), false);
                builder.AddAttribute(4, nameof(RadzenProgressBarCircular.Mode), ProgressBarMode.Indeterminate);
                builder.AddAttribute(5, nameof(RadzenProgressBarCircular.Size), ProgressBarCircularSize.ExtraSmall);
                builder.CloseComponent();
            }
            else
            {
                builder.AddContent(0, GetValue(item));
            }
        };

        HeaderTemplate = builder =>
        {
            builder.OpenComponent<RadzenIcon>(0);
            builder.AddAttribute(1, nameof(RadzenIcon.Icon), Replication
                                                                ? PveAdminUIHelper.Icons.Replication
                                                                : PveAdminUIHelper.Icons.Snapshot);
            builder.CloseComponent();
        };
    }
}
