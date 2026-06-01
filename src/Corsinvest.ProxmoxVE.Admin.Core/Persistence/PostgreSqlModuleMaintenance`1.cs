/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Persistence;

/// <summary>
/// Convenience generic over <see cref="PostgreSqlModuleMaintenance"/> for modules whose
/// DbContext inherits <see cref="ModuleDbContextBase{T}"/>. Resolves the DbContext from
/// the provided scope so the module only writes:
/// <code>
/// public override IModuleMaintenance GetMaintenance(IServiceScope scope)
///     =&gt; new PostgreSqlModuleMaintenance&lt;ModuleDbContext&gt;(scope);
/// </code>
/// </summary>
public class PostgreSqlModuleMaintenance<T> : PostgreSqlModuleMaintenance
    where T : ModuleDbContextBase<T>
{
    public PostgreSqlModuleMaintenance(IServiceScope scope)
        : base(scope.GetRequiredService<T>()) { }
}
