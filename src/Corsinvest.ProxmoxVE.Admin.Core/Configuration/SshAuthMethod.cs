/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SshAuthMethod
{
    [Display(Name = "None")] None = 0,
    [Display(Name = "Password")] Password = 1,
    [Display(Name = "Private Key")] PrivateKey = 2,
    [Display(Name = "Same as WEB API")] SameAsWebApi = 3,
}
