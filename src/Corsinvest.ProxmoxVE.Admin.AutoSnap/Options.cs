/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Notification;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Admin.Core.UI.Options;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap;

public class Options : PveModuleClustersOptions<ModuleClusterOptions> { }

public class ModuleClusterOptions : IClusterName, INotificationChannelsOptions
{
    [Required]
    public string ClusterName { get; set; } = default!;

    [Range(0, int.MaxValue)]
    [Display(Name = "Keep history maintain after deletion snapshot in Proxmox VE")]
    public int KeepHistory { get; set; } = 10;

    [Display(Name = "Notify")]
    public Notify Notify { get; set; } = Notify.None;

    public SearchMode SearchMode { get; set; } = SearchMode.Managed;

    [Display(Name = "Timestamp format")]
    public string TimestampFormat { get; set; } = ProxmoxVE.AutoSnap.Api.Application.DefaultTimestampFormat;

    [Display(Name = "On remove job remove snapshots in Proxmox VE")]
    public bool OnRemoveJobRemoveSnapshots { get; set; } = true;

    [Display(Name = "Max percentage storage (default 95%)")]
    [Range(0, 100)]
    public int MaxPercentageStorage { get; set; } = 95;

    public IEnumerable<string> NotificationChannels { get; set; } = default!;
}
