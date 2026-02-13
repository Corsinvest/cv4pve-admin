/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface IClusterNamesParameters
{
    [Parameter] IEnumerable<string> ClusterNames { get; set; }
    [Parameter] EventCallback<IEnumerable<string>> ClusterNamesChanged { get; set; }
}
