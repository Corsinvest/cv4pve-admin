/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SshAuthMethod
{
    Password = 0,
    PrivateKey = 1
}
