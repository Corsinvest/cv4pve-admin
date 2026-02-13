/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Notifier;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;

public partial class NotifierSettingsDialog<TSettings>(DialogService dialogService) : IDisposable, IModelParameter<TSettings>
    where TSettings : NotifierConfiguration
{
    [Parameter] public TSettings Model { get; set; } = default!;
    [Parameter] public bool IsNew { get; set; }
    [Parameter] public RenderFragment<TSettings> EditContent { get; set; } = default!;

    private EditContext EditContext { get; set; } = default!;
    private string Text => L[IsNew ? "Create" : "Save"];

    protected override void OnInitialized()
    {
        EditContext = new(Model);
        EditContext.OnFieldChanged += HandleFieldChanged!;
    }

    public void Dispose() => EditContext.OnFieldChanged -= HandleFieldChanged!;
    private void HandleFieldChanged(object sender, FieldChangedEventArgs e) => StateHasChanged();
    private void OnSubmit(TSettings model) => dialogService.Close(model);
}
