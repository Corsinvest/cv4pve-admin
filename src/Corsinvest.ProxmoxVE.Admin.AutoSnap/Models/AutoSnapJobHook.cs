/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Entities;
using Corsinvest.ProxmoxVE.AutoSnap.Api;

namespace Corsinvest.ProxmoxVE.Admin.AutoSnap.Models;

public class AutoSnapJobHook : EntityBase<int>, IAggregateRoot<int>
{
    [Required]
    public AutoSnapJob Job { get; set; } = default!;

    [Required]
    public string Description { get; set; } = default!;

    public bool Enabled { get; set; }
    public int Order { get; set; } = 0;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public HookPhase Phase { get; set; } = default!;

    [Required]
    public string Url { get; set; } = default!;

    public AutoSnapJobHookHttpMethod HttpMethod { get; set; } = default!;
    public string Data { get; set; } = default!;
    public bool DataIsKeyValue { get; set; }
}