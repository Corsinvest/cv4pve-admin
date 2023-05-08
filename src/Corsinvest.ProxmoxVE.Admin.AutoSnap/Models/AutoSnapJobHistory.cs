/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;
using Corsinvest.AppHero.Core.Domain.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

public class AutoSnapJobHistory : JobHistory, IAggregateRoot<int>, IClusterName
{
    [Required]
    public int Id { get; set; }

    [Required]
    public string ClusterName { get; set; } = default!;

    [Required]
    public AutoSnapJob Job { get; set; } = default!;

    [NotMapped]
    [DisplayName("Job Id")]
    public int JobIdTmp => Job?.Id ?? -1;
}