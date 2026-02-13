/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;
using Microsoft.Extensions.Localization;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Services;

internal class UpdaterService(IReportService reportService, IStringLocalizer<UpdaterService> L) : IUpdaterService
{
    public MemoryStream GeneratePdf(string clusterName, IEnumerable<ClusterResourceUpdateScanInfo> items)
        => reportService.GeneratePdf(L["Update result of cluster '{0}' Date {1}", clusterName, items.Min(a => a.UpdateScanTimestamp)!]);
}

