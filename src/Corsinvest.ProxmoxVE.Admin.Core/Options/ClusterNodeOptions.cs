/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Options;

public class ClusterNodeOptions
{
    [Required]
    public string IpAddress { get; set; } = default!;

    public int ApiPort { get; set; } = 8006;
    public int SshPort { get; set; } = 22;

    public string ServerId { get; set; } = default!;
    public string SubscriptionId { get; set; } = default!;
}
