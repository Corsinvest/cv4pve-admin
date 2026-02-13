/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public class ClusterSettings : IName, IDescription, IEnabled
{
    [Required] public string Name { get; set; } = default!;
    [Required] public string PveName { get; set; } = default!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClusterType Type { get; set; } = default!;

    [Required] public string Description { get; set; } = default!;
    public int Timeout { get; set; } = 1000;
    public bool Enabled { get; set; } = true;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    //Feature
    public bool AllowCalculateSnapshotSize { get; set; }

    public bool ValidateCertificate { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClusterAccessType AccessType { get; set; } = ClusterAccessType.Credential;

    [Encrypt]
    public string ApiToken { get; set; } = default!;

    public Credential ApiCredential { get; set; } = new();
    //public Credential SshCredential { get; set; } = new();
    public List<ClusterNodeSettings> Nodes { get; set; } = [];

    [JsonIgnore]
    public string DecodeType
        => Type switch
        {
            ClusterType.Cluster => "CLUSTER",
            ClusterType.SingleNode => "NODE",
            _ => "NODE"
        };

    [JsonIgnore]
    public string Icon
        => Type switch
        {
            ClusterType.Cluster => PveAdminUIHelper.Icons.Cluster,
            ClusterType.SingleNode => PveAdminUIHelper.Icons.Node,
            _ => PveAdminUIHelper.Icons.Node
        };

    [JsonIgnore]
    public string FullNamePart1
    {
        get
        {
            var ret = Name == PveName
                    ? Name
                    : $"{Name} ({PveName})";

            return $"{DecodeType}: {ret}";
        }
    }

    [JsonIgnore]
    public string FullName
    {
        get
        {
            var fullName = FullNamePart1;
            if (!string.IsNullOrEmpty(Description)) { fullName = $"{fullName} - {Description}"; }

            return fullName;
        }
    }

    public ClusterNodeSettings? GetNodeSettings(string iPAddress, string host)
        => Nodes.FirstOrDefault(a => a.IPAddress == iPAddress || a.IPAddress.Equals(host, StringComparison.CurrentCultureIgnoreCase));

    [JsonIgnore]
    public string ApiHostsAndPortHA => Nodes.Select(a => $"{a.IPAddress}:{a.ApiPort}").JoinAsString(",");

    //[JsonIgnore]
    //public string SshHostsAndPortHA => Nodes.Select(a => $"{a.IPAddress}:22").JoinAsString(",");

    //[JsonIgnore]
    //public Dictionary<string, object> RuntimeData { get; set; } = [];

    //public T? GetRuntimeData<T>(string key, T? defaultValue = null) where T : class
    //    => RuntimeData.TryGetValue(key, out var value) ? value as T : defaultValue;

    //public void SetRuntimeData<T>(string key, T value) where T : class
    //    => RuntimeData[key] = value;

    public ExtendedData ExtendedData { get; set; } = [];
}
