/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text;
using Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Renci.SshNet;
using OperationResult = FluentResults.Result;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services;

public class ClusterClient(IPveClientFactory pveClientFactory,
                           ClusterSettings clusterSettings,
                           IFusionCache fusionCache,
                           IServiceProvider serviceProvider)
{
    public ClusterSettings Settings => clusterSettings;
    public ClusterCachedData CachedData { get; } = new(pveClientFactory, fusionCache, clusterSettings, serviceProvider);
    public async Task<PveClient> GetPveClientAsync() => await CachedData.GetPveClientAsync();

    public async Task<FluentResults.Result<string>> PopulateSettingsAsync()
    {
        var ret = OperationResult.Ok("All nodes have been inserted and updated!");
        var client = await GetPveClientAsync();
        if (client == null)
        {
            ret = OperationResult.Fail("Credential or host not valid!");
        }
        else
        {
            if (await IsValidVersionAsync(client))
            {
                var info = await client.GetClusterInfoAsync();
                Settings.PveName = info.Name;
                Settings.Type = info.Type;
                if (string.IsNullOrEmpty(Settings.Name)) { Settings.Name = Settings.PveName; }

                var status = await client.Cluster.Status.GetAsync();

                //check new nodes
                foreach (var item in status.Where(a => !string.IsNullOrWhiteSpace(a.IpAddress)))
                {
                    if (Settings.GetNodeSettings(item.IpAddress, item.Name) == null)
                    {
                        Settings.Nodes.Add(new()
                        {
                            IPAddress = item.IpAddress
                        });
                        ret = OperationResult.Ok("New nodes added and updated!");
                    }
                }
            }
            else
            {
                ret = OperationResult.Fail($"Proxmox VE version not valid! Required {PveAdminHelper.MinimalVersion}");
            }
        }

        await CachedData.GetPveClientAsync(true);

        return ret;
    }

    public string GetUrlWebConsole(string node, bool xTermJs)
    {
        var encode = true;
        return encode
                    ? GetUrlWebConsole(node, VmType.Qemu, 0, string.Empty, xTermJs)
                    : PveAdminHelper.GetUrlWebConsole(clusterSettings.Name, node, xTermJs);
    }

    public string GetUrlWebConsole(string node, VmType vmType, long vmId, string vmName, bool xTermJs)
    {
        var encode = true;
        if (encode)
        {
            var key = Guid.NewGuid().ToString();
            fusionCache.Set(key, new WebConsoleInfo(clusterSettings.Name, vmId, node, vmName, vmType, xTermJs), TimeSpan.FromSeconds(10));
            return $"/webconsole/-/{key}";
        }
        else
        {
            return PveAdminHelper.GetUrlWebConsole(clusterSettings.Name,
                                                   node,
                                                   vmType,
                                                   vmId,
                                                   vmName,
                                                   xTermJs);
        }
    }

    private async Task<ConnectionInfo> GetSshConnectionInfoAsync(string node, bool resolveHost)
    {
        var host = resolveHost
                    ? await GetNodeIpAsync(node)
                    : node;
        return new(host, SshCredential.DefaultPort, Settings.SshCredential.Username, GetSshAuthMethod())
        {
            Timeout = TimeSpan.FromMilliseconds(Settings.SshCredential.Timeout)
        };
    }

    private AuthenticationMethod GetSshAuthMethod()
    {
        var cred = Settings.SshCredential;
        if (cred.AuthMethod == SshAuthMethod.PrivateKey)
        {
            var keyData = Encoding.UTF8.GetBytes(cred.PrivateKeyContent);
            using var keyStream = new MemoryStream(keyData);
            var keyFile = string.IsNullOrEmpty(cred.Passphrase)
                            ? new PrivateKeyFile(keyStream)
                            : new PrivateKeyFile(keyStream, cred.Passphrase);
            return new PrivateKeyAuthenticationMethod(cred.Username, keyFile);
        }
        else
        {
            return new PasswordAuthenticationMethod(cred.Username, cred.Password);
        }
    }

    private async Task<string> GetNodeIpAsync(string node)
    {
        var client = await GetPveClientAsync();
        var (host, ipAddress) = (await client.GetHostAndIpAsync()).FirstOrDefault(a => a.Key == node);
        if (Settings.GetNodeSettings(ipAddress, host) == null) { throw new AdminException("Host not found!"); }
        return ipAddress;
    }

    public async Task<FluentResults.Result<string>> TestSshAsync()
    {
        if (!Settings.SshCredential.IsConfigured) { return OperationResult.Ok("SSH not configured, skipped."); }

        var errors = new List<string>();
        foreach (var node in Settings.Nodes)
        {
            try
            {
                using var client = new SshClient(await GetSshConnectionInfoAsync(node.IPAddress, false));
                await client.ConnectAsync(CancellationToken.None);
                client.Disconnect();
            }
            catch (Exception ex)
            {
                errors.Add($"{node.IPAddress}: {ex.Message}");
            }
        }

        return errors.Count == 0
            ? OperationResult.Ok("SSH connection successful on all nodes.")
            : OperationResult.Fail(errors.JoinAsString("\n"));
    }

    private static async Task<bool> IsValidVersionAsync(PveClient client)
        => Version.TryParse((await client.Version.GetAsync()).Version.Split("-")[0], out var version)
            && version >= PveAdminHelper.MinimalVersion;

    public async Task<SshClient> GetSshClientAsync(string node, bool resolveHost) => new(await GetSshConnectionInfoAsync(node, resolveHost));
    public async Task<SftpClient> GetSftpClientAsync(string node, bool resolveHost) => new(await GetSshConnectionInfoAsync(node, resolveHost));

    public async Task<IReadOnlyList<SshCommandResult>> SshExecuteAsync(string node,
                                                                       bool resolveHost,
                                                                       IEnumerable<string> commands,
                                                                       bool stopOnError = true)
    {
        if (!Settings.SshCredential.IsConfigured)
        {
            return [new(string.Empty,
                        -1,
                        string.Empty,
                        "SSH not available for this cluster! Please configure SSH in the cluster settings.",
                        true)];
        }

        var results = new List<SshCommandResult>();

        using var sshClient = await GetSshClientAsync(node, resolveHost);
        await sshClient.ConnectAsync(CancellationToken.None);

        try
        {
            foreach (var command in commands)
            {
                using var cmd = sshClient.CreateCommand(command);
                var stdOut = cmd.Execute();
                var exitCode = cmd.ExitStatus ?? -1;
                var stdErr = cmd.Error;

                results.Add(new(command, exitCode, stdOut, stdErr));

                if (stopOnError && exitCode != 0) { break; }
            }
        }
        finally
        {
            sshClient.Disconnect();
        }

        return results;
    }
}
