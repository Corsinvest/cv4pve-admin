namespace Corsinvest.ProxmoxVE.Admin.Core.Components.DataGrid;

public class SelectionColumn<TItem> : RadzenDataGridColumn<TItem> where TItem : notnull
{
    [Parameter] public bool ShowAll { get; set; } = true;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        Width = "60px";
        Sortable = false;
        Filterable = false;
        Groupable = false;
        Reorderable = false;
        Resizable = false;

        HeaderTemplate = builder =>
        {
            if (ShowAll)
            {
                builder.OpenComponent<RadzenCheckBox<bool?>>(0);
                builder.AddAttribute(1, nameof(RadzenCheckBox<>.TriState), false);

                builder.AddAttribute(2,
                                     nameof(RadzenCheckBox<>.Value),
                                     !Grid.Value!.Any()
                                        ? false
                                        : !Grid.Data!.All(a => Grid.Value!.Contains(a))
                                            ? null
                                            : Grid.Data!.Any(a => Grid.Value!.Contains(a)));

                builder.AddAttribute(3,
                                     nameof(RadzenCheckBox<>.Change),
                                     EventCallback.Factory.Create<bool?>(this, a => Grid.Value = [.. a == true ? Grid.Data! : []]));
                builder.CloseComponent();
            }
        };

        Template = item => builder =>
        {
            builder.OpenComponent<RadzenCheckBox<bool>>(0);
            builder.AddAttribute(1, nameof(RadzenCheckBox<>.TriState), false);
            builder.AddAttribute(2, nameof(RadzenCheckBox<>.Value), Grid.Value!.Contains(item));
            builder.CloseComponent();
        };
    }
}
