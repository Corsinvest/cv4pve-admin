/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm.Backup;

public class BackupModel
{
    [Required] public string Storage { get; set; } = default!;
    public string Mode { get; set; } = "snapshot";
    public string Compress { get; set; } = "zstd";
    public bool Protected { get; set; }
    public string? Notes { get; set; }
    public IEnumerable<string> Storages { get; set; } = [];
}
