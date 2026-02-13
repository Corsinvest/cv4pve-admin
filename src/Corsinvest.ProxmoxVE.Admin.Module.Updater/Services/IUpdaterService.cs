/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Services;

public interface IUpdaterService
{
    MemoryStream GeneratePdf(string clusterName, IEnumerable<ClusterResourceUpdateScanInfo> items);
}
