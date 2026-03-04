/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public class ClusterSettings : IName, IDescription, IEnabled, IValidatableObject
{
    [Required] public string Name { get; set; } = default!;
    [Required] public string PveName { get; set; } = default!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClusterType Type { get; set; } = default!;

    [Required] public string Description { get; set; } = default!;
    public bool Enabled { get; set; } = true;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    //Feature
    public bool AllowCalculateSnapshotSize
    {
        get => SshIsConfigured && field;
        set;
    }

    public WebApiCredential WebApi { get; set; } = new();
    public SshCredential SshCredential { get; set; } = new();
    public List<ClusterNodeSettings> Nodes { get; set; } = [];

    #region Legacy migration — set-only, write into WebApi
    [JsonPropertyName("Timeout")]
    public int Timeout_Legacy { set => WebApi.Timeout = value; }

    [JsonPropertyName("ValidateCertificate")]
    public bool ValidateCertificate_Legacy { set => WebApi.ValidateCertificate = value; }

    [JsonPropertyName("AccessType")]
    public ClusterAccessType AccessType_Legacy { set => WebApi.AccessType = value; }

    [JsonPropertyName("ApiToken")]
    public string ApiToken_Legacy { set => WebApi.ApiToken = value; }

    [JsonPropertyName("ApiCredential")]
    public Credential ApiCredential_Legacy { set { WebApi.Username = value.Username; WebApi.Password = value.Password; } }
    #endregion

    //[JsonIgnore]
    //public Dictionary<string, object> RuntimeData { get; set; } = [];

    //public T? GetRuntimeData<T>(string key, T? defaultValue = null) where T : class
    //    => RuntimeData.TryGetValue(key, out var value) ? value as T : defaultValue;

    //public void SetRuntimeData<T>(string key, T value) where T : class
    //    => RuntimeData[key] = value;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!ApplicationHelper.IsValidClusterName(Name))
        {
            yield return new ValidationResult($"'{Name}' is not a valid cluster name.", [nameof(Name)]);
        }
    }

    public ExtendedData ExtendedData { get; set; } = [];

    [JsonIgnore]
    public bool SshIsConfigured => SshCredential.IsConfigured;

    [JsonIgnore]
    public string TypeLabel
        => Type switch
        {
            ClusterType.Cluster => "CLUSTER",
            ClusterType.SingleNode => "NODE",
            _ => "NODE"
        };

    [JsonIgnore]
    public string TypeIcon
        => Type switch
        {
            ClusterType.Cluster => PveAdminUIHelper.Icons.Cluster,
            ClusterType.SingleNode => PveAdminUIHelper.Icons.Node,
            _ => PveAdminUIHelper.Icons.Node
        };

    [JsonIgnore]
    public string DisplayName
    {
        get
        {
            var ret = Name == PveName
                    ? Name
                    : $"{Name} ({PveName})";

            return ret;
        }
    }

    [JsonIgnore]
    public string FullDisplayName
    {
        get
        {
            var fullName = DisplayName;
            if (!string.IsNullOrEmpty(Description)) { fullName = $"{fullName} - {Description}"; }

            return fullName;
        }
    }

    [JsonIgnore]
    public string ApiHostsAndPortHA
        => Nodes.Select(a => $"{a.IPAddress}:{a.ApiPort}").JoinAsString(",");
}
