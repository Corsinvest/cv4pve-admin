using System.Reflection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

internal class ModuleService : IModuleService
{
    public IEnumerable<ModuleBase> Modules { get; }

    public ModuleService(IEnumerable<ModuleBase> modules)
    {
        Modules = modules;
        IModuleService.InitializeCache(modules);
    }

    public T? Get<T>() where T : ModuleBase
        => IModuleService.GetCached<T>();

    public ModuleBase? Get(Type type)
        => !typeof(ModuleBase).IsAssignableFrom(type)
            ? throw new ArgumentException("Type must derive from ModuleBase", nameof(type))
            : Modules.FirstOrDefault(a => a.GetType() == type);

    public ModuleBase? Get(string @class) => Modules.FirstOrDefault(a => a.Type == @class);
    public ModuleBase? GetBySlug(string slug) => Modules.FirstOrDefault(a => a.Slug == slug);

    public IEnumerable<Assembly> Assemblies
    {
        get
        {
            var ret = new List<Assembly>();

            foreach (var module in Modules)
            {
                var moduleType = module.GetType();
                ret.Add(moduleType.Assembly);

                // Add base type assemblies (to support inheritance like System.Enterprise -> System)
                var baseType = moduleType.BaseType;
                while (baseType != null && baseType != typeof(ModuleBase) && baseType != typeof(object))
                {
                    ret.Add(baseType.Assembly);
                    baseType = baseType.BaseType;
                }
            }

            ret.Add(GetType().Assembly);
            return ret.Distinct();
        }
    }
}
