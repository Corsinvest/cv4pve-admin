/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class EditDialog<TModelEditor>(DialogService dialogService) : IDisposable, IModelParameter<object>
{
    [EditorRequired, Parameter] public object Model { get; set; } = default!;
    [Parameter] public EditDialogMode Mode { get; set; }
    [Parameter] public Func<object, bool, Task<bool>>? OnSubmiting { get; set; }

    private EditContext? EditContext { get; set; }
    private Dictionary<string, object> EditorParameters => new() { [nameof(IModelParameter<>.Model)] = Model };

    protected override void OnInitialized()
    {
        ArgumentNullException.ThrowIfNull(Model);

        EditContext?.OnFieldChanged -= HandleFieldChanged;
        EditContext = new(Model);
        EditContext.OnFieldChanged += HandleFieldChanged;
    }

    public void Dispose() => EditContext?.OnFieldChanged -= HandleFieldChanged;
    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e) => StateHasChanged();

    private async Task OnSubmitAsync(object model)
    {
        if (OnSubmiting == null || await OnSubmiting(model, Mode == EditDialogMode.Create))
        {
            dialogService.Close(model);
        }
    }
}
