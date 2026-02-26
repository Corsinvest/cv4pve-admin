/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json.Serialization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Configuration;

public class SshCredential : Credential
{
    public const int DefaultPort = 22;

    public SshAuthMethod AuthMethod { get; set; } = SshAuthMethod.Password;
    public int Timeout { get; set; } = 5000;

    [Encrypt] public string PrivateKeyContent { get; set; } = default!;
    [Encrypt] public string Passphrase { get; set; } = default!;

    [JsonIgnore]
    public bool IsConfigured => !string.IsNullOrEmpty(Username)
                                && ((AuthMethod == SshAuthMethod.Password && !string.IsNullOrEmpty(Password))
                                    || (AuthMethod == SshAuthMethod.PrivateKey && !string.IsNullOrEmpty(PrivateKeyContent)));
}
