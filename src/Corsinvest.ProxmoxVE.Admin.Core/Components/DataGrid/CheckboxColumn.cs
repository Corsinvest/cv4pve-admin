namespace Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;

public class CheckboxColumn<TItem> : RadzenDataGridColumn<TItem> where TItem : notnull
{
    protected override void OnInitialized()
    {
        base.OnInitialized();

        Template = item => builder =>
        {
            builder.OpenComponent<RadzenCheckBox<bool>>(0);
            builder.AddAttribute(1, nameof(RadzenCheckBox<>.Value), Convert.ToBoolean(GetValue(item)));
            builder.AddAttribute(2, nameof(RadzenCheckBox<>.Disabled), true);
            builder.CloseComponent();
        };
    }
}
