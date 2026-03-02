/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Module.SystemReport.Models;

[Flags]
public enum VmFeature
{
    All = 0,
    Network = 1,
    Disks = 2,
    [Display(Name = "QEMU Guest Info")] QemuGuestInfo = 4,
    [Display(Name = "RRD Data")] RrdData = 8,
    Backup = 16,
    Replications = 32,
    Snapshots = 64,
    [Display(Name = "Snapshots Size")] SnapshotsSize = 128
}
