namespace Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;

public class BaseColumn<TItem> : RadzenDataGridColumn<TItem> where TItem : notnull
{
    [Parameter] public bool UseProgressBarPercentage { get; set; }
    [Parameter] public Func<TItem, string>? TooltipBarPercentage { get; set; }

    [Parameter] public bool ShowWating { get; set; }
    [Parameter] public Func<TItem, bool>? ShowWatingFunc { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        var isBool = typeof(TItem).GetProperty(Property) != null
                        && typeof(TItem).GetProperty(Property)!.PropertyType == typeof(bool);

        if (!string.IsNullOrEmpty(Property))
        {
            if (string.IsNullOrEmpty(Title)) { Title = TypeHelper.GetDisplayDescription<TItem>(Property); }
            if (!UseProgressBarPercentage)
            {
                if (string.IsNullOrEmpty(FormatString)) { FormatString = TypeHelper.GetDisplayFormatAttribute<TItem>(Property)!; }
                if (!string.IsNullOrEmpty(FormatString)) { FormatProvider ??= FormatterHelper.FormatProvider; }
            }

            Template = item => builder =>
            {
                if (item != null)
                {
                    if (ShowWating || (ShowWatingFunc != null && ShowWatingFunc(item)))
                    {
                        builder.OpenComponent<RadzenProgressBarCircular>(0);
                        builder.AddAttribute(1, nameof(RadzenProgressBarCircular.ProgressBarStyle), ProgressBarStyle.Primary);
                        builder.AddAttribute(2, nameof(RadzenProgressBarCircular.Value), 100d);
                        builder.AddAttribute(3, nameof(RadzenProgressBarCircular.ShowValue), false);
                        builder.AddAttribute(4, nameof(RadzenProgressBarCircular.Mode), ProgressBarMode.Indeterminate);
                        builder.AddAttribute(5, nameof(RadzenProgressBarCircular.Size), ProgressBarCircularSize.ExtraSmall);
                        builder.CloseComponent();
                    }
                    else if (isBool)
                    {
                        var value = Convert.ToBoolean(GetValue(item));
                        builder.OpenComponent<RadzenIcon>(0);
                        builder.AddAttribute(1, nameof(RadzenIcon.Icon), value ? "check" : string.Empty);
                        builder.AddAttribute(2, nameof(RadzenIcon.IconColor), value ? Colors.Success : Colors.Danger);
                        builder.CloseComponent();
                    }
                    else if (UseProgressBarPercentage)
                    {
                        var value = Convert.ToDouble(GetValue(item)) * 100;

                        builder.OpenComponent<RadzenProgressBar>(0);
                        builder.AddAttribute(1, nameof(RadzenProgressBar.Value), Math.Round(value, 1));
                        builder.AddAttribute(2, "class", "rz-w-100");
                        //builder.AddAttribute(2, nameof(RadzenProgressBar.Style), "width:100%");
                        builder.AddAttribute(3,
                                             nameof(RadzenProgressBar.ProgressBarStyle),
                                             PveAdminUIHelper.ToProgressBarStyle(PveAdminUIHelper.GetColorRange(value)));

                        if (TooltipBarPercentage != null)
                        {
                            builder.AddAttribute(4, "title", TooltipBarPercentage.Invoke(item));
                        }

                        builder.CloseComponent();
                    }
                    else
                    {
                        builder.AddContent(0, GetValue(item));
                    }
                }
            };
        }
    }
}
