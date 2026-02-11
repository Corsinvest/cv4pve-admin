namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class InfoScheduler<TModule>(IDialogServiceEx dialogServiceEx) : IClusterName where TModule : ModuleBase
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;
    [Parameter] public IJobSchedule JobSchedule { get; set; } = default!;

    private async Task OpenSettingsAsync() => await dialogServiceEx.OpenSettingsAsync<TModule>(ClusterName);
}
