/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Reflection;

namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public interface IModuleService
{
    IEnumerable<ModuleBase> Modules { get; }
    ModuleBase? Get(string @class);
    ModuleBase? Get(Type moduleType);
    T? Get<T>() where T : ModuleBase;
    ModuleBase? GetBySlug(string slug);
    IEnumerable<Assembly> Assemblies { get; }

    private static IEnumerable<ModuleBase> _cachedModules = [];

    static void InitializeCache(IEnumerable<ModuleBase> modules) => _cachedModules = modules;

    static T? GetCached<T>() where T : ModuleBase
        => _cachedModules.FirstOrDefault(a => typeof(T).IsAssignableFrom(a.GetType())) as T;
}
