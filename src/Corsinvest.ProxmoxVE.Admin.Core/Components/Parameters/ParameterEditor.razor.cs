using Corsinvest.ProxmoxVE.Admin.Core.Models.Parameters;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Parameters;

public partial class ParameterEditor
{
    [Parameter] public ParameterMetadata Parameter { get; set; } = default!;

    [Parameter] public object? Value { get; set; }
    [Parameter] public EventCallback<object?> ValueChanged { get; set; } = default!;

    [Parameter] public bool IsValid { get; set; }
    [Parameter] public EventCallback<bool> IsValidChanged { get; set; } = default!;

    [Parameter] public Func<ParameterMetadata, Task<DataSourceContext>>? GetDataSourceContext { get; set; }

    private DataSourceResult? DataSourceCache;

    protected override async Task OnInitializedAsync()
    {
        Value = Parameter.Type switch
        {
            ParameterType.Bool => Parameter.DefaultValue as bool? ?? false,
            ParameterType.Number => Parameter.DefaultValue as int?,
            ParameterType.Date or ParameterType.Time or ParameterType.DateTime => Parameter.DefaultValue as DateTime?,
            _ => Parameter.DefaultValue
        };

        if (Parameter.Type == ParameterType.Select && Parameter.Options?.DataSource != null)
        {
            await LoadDataSourceAsync();
        }

        await Validate();
    }

    private async Task LoadDataSourceAsync()
    {
        if (DataSourceCache != null) { return; }

        var ctx = GetDataSourceContext == null
                    ? new([])
                    : await GetDataSourceContext(Parameter);

        DataSourceCache = await Parameter.Options!.DataSource!(ctx);
    }

    public async Task InvlaidateCacheAsync()
    {
        DataSourceCache = null;
        await LoadDataSourceAsync();
    }

    private async Task ValueChangedAsync(object? value)
    {
        Value = value;
        if (ValueChanged.HasDelegate) { await ValueChanged.InvokeAsync(Value); }

        await Validate();

        StateHasChanged();
    }

    public async Task Validate()
    {
        var valid = IsValueValid();

        if (IsValid != valid)
        {
            IsValid = valid;
            if (IsValidChanged.HasDelegate)
            {
                await IsValidChanged.InvokeAsync(IsValid);
            }
        }
    }

    private bool IsValueValid()
        => !Parameter.Required
            || Parameter.Type switch
            {
                ParameterType.Bool => true,
                ParameterType.Number => Value is int,
                ParameterType.Decimal => Value is double,
                ParameterType.Date or ParameterType.Time or ParameterType.DateTime => Value is DateTime,
                ParameterType.Text or ParameterType.Password => Value is string s && !string.IsNullOrWhiteSpace(s),
                ParameterType.Select => Value != null,
                ParameterType.MultiSelect => Value is IEnumerable<object> e && e.Any(),
                _ => Value != null
            };
}
