/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class EditDialog<TModelEditor>(DialogService dialogService) : IDisposable, IModelParameter<object>
{
    [EditorRequired, Parameter] public object Model { get; set; } = default!;
    [Parameter] public bool IsNew { get; set; }
    [Parameter] public Func<object, bool, Task<bool>> OnSubmiting { get; set; } = (_, _) => Task.FromResult(true);

    private EditContext? EditContext { get; set; }
    private Dictionary<string, object> EditorParameters => new() { [nameof(IModelParameter<>.Model)] = Model };
    private string ButtonText => L[IsNew ? "Create" : "Save"];

    protected override void OnInitialized()
    {
        ArgumentNullException.ThrowIfNull(Model);

        if (EditContext != null)
        {
            EditContext.OnFieldChanged -= HandleFieldChanged;
        }

        EditContext = new(Model);
        EditContext.OnFieldChanged += HandleFieldChanged;
    }

    public void Dispose()
    {
        if (EditContext != null)
        {
            EditContext.OnFieldChanged -= HandleFieldChanged;
        }
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e) => StateHasChanged();

    private async Task OnSubmitAsync(object model)
    {
        if (await OnSubmiting(model, IsNew)) { dialogService.Close(model); }
    }
}
