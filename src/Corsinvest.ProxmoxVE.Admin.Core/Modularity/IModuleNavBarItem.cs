/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public interface IModuleNavBarItem
{
    int OrderIndex { get; }
    string ReferenceId { get; }
    Type ModuleType { get; }
    ModuleLinkBase Create(ModuleBase module);
}
