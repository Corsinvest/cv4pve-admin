/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Modularity;

public interface IModuleWidget<TSettings> : IClusterNames,
                                            IRefreshableData,
                                            ISettingsParameter<TSettings>
{
    bool InEditing { get; set; }
}
