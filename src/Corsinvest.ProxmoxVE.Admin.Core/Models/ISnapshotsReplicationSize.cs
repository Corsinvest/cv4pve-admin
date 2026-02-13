/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface ISnapshotsReplicationSize
{
    [Display(Name = "Snapshots Replication Size")]
    [DisplayFormat(DataFormatString = FormatHelper.DataFormatBytes)]
    double SnapshotsReplicationSize { get; set; }
}
