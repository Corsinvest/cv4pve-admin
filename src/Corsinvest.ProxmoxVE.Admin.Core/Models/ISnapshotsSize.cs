/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.Models;

public interface ISnapshotsSize
{
    [Display(Name = "Snapshots Size")]
    [DisplayFormat(DataFormatString = "{0:" + FormatHelper.FormatBytes + "}")]
    double SnapshotsSize { get; set; }
}