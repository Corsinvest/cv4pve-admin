/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Module.Updater.Models;

public class ClusterResourceUpdateScanInfo : ClusterResource
{
    public UpdateInfoStatus UpdateScanStatus { get; set; } = UpdateInfoStatus.InScan;

    [Display(Name = "Normal")]
    public bool UpdateNormalAvailable { get; set; }

    [Display(Name = "Security")]
    public bool UpdateSecurityAvailable { get; set; }

    [Display(Name = "Reboot")]
    public bool UpdateRequireReboot { get; set; }

    [Display(Name = "Last Scan")]
    public DateTime? UpdateScanTimestamp { get; set; }

    public string Error { get; set; } = default!;
}
