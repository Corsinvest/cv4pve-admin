namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public interface IModuleWidget<TSettings> : IClusterNames,
                                            IRefreshableData,
                                            ISettingsParameter<TSettings>
{
    bool InEditing { get; set; }
}
