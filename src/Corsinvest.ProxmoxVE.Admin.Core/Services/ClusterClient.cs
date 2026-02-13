/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Clients.Pve;
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
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
            if (await client.CheckClusterIsValidVersionAsync())
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

    //public async Task<SshClient?> GetSshClientAsync(string node)
    //{
    //    var client = await GetPveClientAsync();
    //    var (host, ipAddress) = (await client.GetHostAndIpAsync()).FirstOrDefault(a => a.Key == node);
    //    var info = Settings.GetNodeSettings(ipAddress, host);
    //    return info == null
    //            ? throw new AdminException("Host not found!")
    //            : new SshClient(ipAddress,
    //                            22,
    //                            Settings.SshCredential.Username,
    //                            Settings.SshCredential.Password);
    //}

    //public async Task<SftpClient?> GetSftpClientAsync(string node)
    //{
    //    var client = await GetPveClientAsync();
    //    var (host, ipAddress) = (await client.GetHostAndIpAsync()).FirstOrDefault(a => a.Key == node);
    //    var info = Settings.GetNodeSettings(ipAddress, host);
    //    return info == null
    //            ? throw new AdminException("Host not found!")
    //            : new SftpClient(ipAddress,
    //                             22,
    //                             Settings.SshCredential.Username,
    //                             Settings.SshCredential.Password);
    //}

    //public async Task<IEnumerable<(int ExitCode, string StdOut, string Error)>> SshExecuteAsync(string node, IEnumerable<string> commands)
    //{
    //    var ret = new List<(int ExitCode, string StdOut, string Error)>();

    //    using var sshClient = (await GetSshClientAsync(node))!;
    //    await sshClient.ConnectAsync(CancellationToken.None);
    //    foreach (var command in commands)
    //    {
    //        using var cmd = sshClient.CreateCommand(command);
    //        ret.Add((Convert.ToInt32(cmd.ExitStatus), cmd.Execute(), cmd.Error));
    //    }
    //    sshClient.Disconnect();
    //    return ret;
    //}
}
