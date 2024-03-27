/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Domain.Models;
using Corsinvest.ProxmoxVE.Admin.Core.Options;
using Newtonsoft.Json;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configurations;

public class ClusterOptions
{
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Description { get; set; } = default!;

    [Display(Name = "API Token")]
    public string ApiToken { get; set; } = default!;

    [Display(Name = "Timeout (millisec)")]
    public int Timeout { get; set; } = 1000;

    [Display(Name = "Calculate snapshot size (require ssh configuration)")]
    public bool CalculateSnapshotSize { get; set; }

    public bool UseApiToken { get; set; }
    public Credential ApiCredential { get; } = new();
    public Credential SshCredential { get; } = new();
    public List<ClusterNodeOptions> Nodes { get; set; } = [];

    [JsonIgnore]
    public string FullName => $"{Type}: {Name} - {Description}";

    public ClusterNodeOptions? GetNodeOptions(string ipAddress, string host)
        => Nodes.FirstOrDefault(a => a.IpAddress == ipAddress || a.IpAddress.Equals(host, StringComparison.CurrentCultureIgnoreCase));

    [JsonIgnore]
    public string ApiHostsAndPortHA => Nodes.Select(a => $"{a.IpAddress}:{a.ApiPort}").JoinAsString(",");

    [JsonIgnore]
    public string SshHostsAndPortHA => Nodes.Select(a => $"{a.IpAddress}:{a.SshPort}").JoinAsString(",");
}