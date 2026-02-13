/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;

public partial class ModuleSettingsDialog(ISettingsService settingsService,
                                          IServiceScopeFactory scopeFactory,
                                          DialogService dialogService) : IDisposable, IClusterName
{
    [Parameter] public string ClusterName { get; set; } = default!;
    [Parameter] public ModuleBase Module { get; set; } = default!;

    private EditContext EditContext { get; set; } = default!;
    private IClusterName Settings { get; set; } = default!;

    private Dictionary<string, object> ContentParameters { get; set; } = [];

    protected override void OnInitialized()
    {
        Settings = (IClusterName)settingsService.GetForModule(Module, ClusterName);
        EditContext = new(Settings);
        EditContext.OnFieldChanged += HandleFieldChanged!;
        ContentParameters = new() { [nameof(IModelParameter<>.Model)] = Settings };
    }

    public void Dispose() => EditContext.OnFieldChanged -= HandleFieldChanged!;
    private void HandleFieldChanged(object sender, FieldChangedEventArgs e) => StateHasChanged();

    private async Task OnSubmitAsync(object model)
    {
        await settingsService.SetAsync(Module, ClusterName, Settings);

        using var scope = scopeFactory.CreateScope();
        await Module.RefreshSettingsEventAsync(scope);

        dialogService.Close(model);
    }
}
