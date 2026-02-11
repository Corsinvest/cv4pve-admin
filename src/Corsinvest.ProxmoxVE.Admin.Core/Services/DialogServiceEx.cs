using Corsinvest.ProxmoxVE.Admin.Core.Components.Settings;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class DialogServiceEx(IStringLocalizer<DialogServiceEx> L,
                             DialogService dialogService,
                             IModuleService moduleService) : IDialogServiceEx
{
    public Task OpenSettingsAsync<T>(string clusterName) where T : ModuleBase => OpenSettingsAsync(moduleService.Get<T>()!, clusterName);

    public async Task OpenSettingsAsync(ModuleBase module, string clusterName)
        => await dialogService.OpenSideExAsync<ModuleSettingsDialog>(L["Settings for "].Value + L[module.Link!.Text].Value,
                                                                            new()
                                                                            {
                                                                                [nameof(ModuleSettingsDialog.Module)] = module,
                                                                                [nameof(ModuleSettingsDialog.ClusterName)] = clusterName
                                                                            },
                                                                            new()
                                                                            {
                                                                                CloseDialogOnOverlayClick = true
                                                                            });
}
