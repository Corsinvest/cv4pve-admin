using Radzen.Blazor.Rendering;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class CronEditor
{
    [Parameter] public string Expression { get; set; } = default!;
    [Parameter] public EventCallback<string> ExpressionChanged { get; set; } = default!;
    [Parameter] public bool IsValid { get; set; } = default!;
    [Parameter] public EventCallback<bool> IsValidChanged { get; set; } = default!;
    [Parameter] public DateTimeOffset? NextOccurrence { get; set; } = default!;
    [Parameter] public EventCallback<DateTimeOffset?> NextOccurrenceChanged { get; set; } = default!;
    [Parameter] public string Descriptor { get; set; } = default!;
    [Parameter] public EventCallback<string?> DescriptorChanged { get; set; } = default!;
    [Parameter] public bool AllowEditor { get; set; }

    private string Id { get; set; } = UniqueID!;
    private Popup PopupRef { get; set; } = default!;
    private RadzenButton ButtonRef { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) { await UpdateAsync(); }
    }

    private async Task UpdateAsync()
    {
        IsValid = CronHelper.IsValid(Expression);
        NextOccurrence = CronHelper.NextOccurrence(Expression);
        Descriptor = CronHelper.GetDescription(Expression);

        if (NextOccurrenceChanged.HasDelegate) { await NextOccurrenceChanged.InvokeAsync(NextOccurrence); }
        if (DescriptorChanged.HasDelegate) { await DescriptorChanged.InvokeAsync(Descriptor); }
        if (IsValidChanged.HasDelegate) { await IsValidChanged.InvokeAsync(IsValid); }
        if (ExpressionChanged.HasDelegate) { await ExpressionChanged.InvokeAsync(Expression); }

        StateHasChanged();
    }
}
