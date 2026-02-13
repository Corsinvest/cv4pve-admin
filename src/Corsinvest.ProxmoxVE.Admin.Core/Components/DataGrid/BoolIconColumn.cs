/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;

public class BoolIconColumn<TItem> : RadzenDataGridColumn<TItem> where TItem : notnull
{
    [Parameter] public string IconStyle { get; set; } = string.Empty;
    [Parameter] public string IconTrue { get; set; } = "check";
    [Parameter] public string IconColorTrue { get; set; } = string.Empty;// Colors.Success;
    [Parameter] public string IconFalse { get; set; } = "close";
    [Parameter] public string IconColorFalse { get; set; } = string.Empty;// Colors.Danger;
    [Parameter] public bool Inverted { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Property))
        {
            Title = TypeHelper.GetDisplayDescription<TItem>(Property);
        }

        if (string.IsNullOrEmpty(Width)) { Width = "150px"; }

        Template = item => builder =>
        {
            var value = GetValue(item);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                var valueBool = Convert.ToBoolean(value);
                if (Inverted) { valueBool = !valueBool; }
                builder.OpenComponent<RadzenIcon>(0);
                builder.AddAttribute(1, nameof(RadzenIcon.Icon), valueBool ? IconTrue : IconFalse);
                builder.AddAttribute(2, nameof(RadzenIcon.IconColor), valueBool ? IconColorTrue : IconColorFalse);

                if (!string.IsNullOrEmpty(IconStyle))
                {
                    builder.AddAttribute(3, nameof(RadzenIcon.Style), IconStyle);
                }
                builder.CloseComponent();
            }
        };

        TextAlign = TextAlign.Center;
    }
}
