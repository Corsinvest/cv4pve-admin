/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.AutoSnap.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.AutoSnap.Models;

public class JobWebHook : IId, IEnabled, IDescription
{
    public int Id { get; set; }
    [Required] public string Description { get; set; } = default!;
    public bool Enabled { get; set; }
    public bool IgnoreSslCertificate { get; set; }
    public int OrderIndex { get; set; }
    public HookPhase Phase { get; set; } = default!;
    [Required] public string Url { get; set; } = default!;
    public AutoSnapJobHookHttpMethod Method { get; set; } = default!;
    public string Header { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}
