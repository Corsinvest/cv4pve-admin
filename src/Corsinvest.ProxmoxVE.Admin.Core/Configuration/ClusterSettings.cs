/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public class ClusterSettings : IName, IDescription, IEnabled
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
        get => SshCredential.IsConfigured && field;
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

    public ExtendedData ExtendedData { get; set; } = [];
}
