namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public interface IDialogServiceEx
{
    Task OpenSettingsAsync(ModuleBase module, string clusterName);
    Task OpenSettingsAsync<T>(string clusterName) where T : ModuleBase;
}
