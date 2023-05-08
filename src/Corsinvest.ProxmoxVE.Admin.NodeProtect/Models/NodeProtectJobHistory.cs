/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;
using Corsinvest.AppHero.Core.Domain.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;
using System.ComponentModel.DataAnnotations.Schema;

namespace Corsinvest.ProxmoxVE.Admin.NodeProtect.Models;

public class NodeProtectJobHistory : JobHistory, IAggregateRoot<int>, IClusterName
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string ClusterName { get; set; } = default!;

    [Required]
    public string JobId { get; set; } = default!;

    [Required]
    public string IpAddress { get; set; } = default!;
    public long Size { get; set; }

    [NotMapped]
    [Display(Name = "Size")]
    public string SizeText => FormatHelper.FromBytes(Size);

    public string FileName { get; set; } = default!;

    public string GetPath() => Helper.GetPath(ClusterName, JobId, FileName);
    public string GetDirectoryWorkJobId() => Helper.GetDirectoryWorkJobId(ClusterName, JobId);
}