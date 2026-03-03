/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

[Flags]
public enum NodeFeature
{
    All = 0,
    Services = 1,
    Network = 2,
    Replications = 4,
    Disks = 8,
    [Display(Name = "Smart Disks")] SmartDisks = 16,
    [Display(Name = "RRD Data")] RrdData = 32,
    [Display(Name = "APT Info")] AptInfo = 64,
    [Display(Name = "Packages Versions")] PackagesVersions = 128,
    [Display(Name = "Snapshots Size")] SnapshotsSize = 256
}
