/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public class WebApiCredential : Credential
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ClusterAccessType AccessType { get; set; } = ClusterAccessType.Credential;

    [Encrypt]
    public string ApiToken { get; set; } = default!;

    public int Timeout { get; set; } = 1000;

    public bool ValidateCertificate { get; set; }
}
