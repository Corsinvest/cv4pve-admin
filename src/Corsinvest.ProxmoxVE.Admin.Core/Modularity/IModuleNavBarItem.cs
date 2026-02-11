namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public interface IModuleNavBarItem
{
    int OrderIndex { get; }
    string ReferenceId { get; }
    Type ModuleType { get; }
    ModuleLinkBase Create(ModuleBase module);
}
