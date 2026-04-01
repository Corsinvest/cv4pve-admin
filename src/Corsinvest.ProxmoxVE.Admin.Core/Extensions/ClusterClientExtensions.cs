/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using System.Text.Json.Serialization;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using OperationResult = FluentResults.Result;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class ClusterClientExtensions
{
    public static async Task<IEnumerable<ResourceUsageItem>> GetResourceUsage(this ClusterClient clusterClient,
                                                                          IStringLocalizer L,
                                                                          bool includeSnapshots)
    {
        var resources = await clusterClient.CachedData.GetResourcesAsync(false);
        var result = resources.GetResourceUsage(L).ToList();

        if (includeSnapshots)
        {
            var disks = await clusterClient.CachedData.GetDiskSnapshotInfosAsync(false);
            var snapshotSize = disks.SelectMany(a => a.Snapshots).Sum(a => a.Size);

            var allStorage = resources.Where(a => a.ResourceType == ClusterResourceType.Storage && a.IsAvailable);
            var storages = allStorage.Where(a => !a.Shared).ToList();
            storages.AddRange(allStorage.Where(a => a.Shared).DistinctBy(a => a.Storage));

            result.Add(new()
            {
                Name = L["Snapshot"],
                Group = "Snapshot",
                Usage = Math.Round(snapshotSize / storages.Sum(a => a.DiskSize) * 100, 1),
                Info = FormatHelper.FromBytes(snapshotSize)
            });
        }

        return result;
    }

    public static async Task<FluentResults.Result<string>> VmExecNativeAsync(this ClusterClient clusterClient,
                                                                             string node,
                                                                             VmType vmType,
                                                                             long vmId,
                                                                             string scriptLinux,
                                                                             string scriptWindows,
                                                                             int timeoutMs = 120000)
    {
        if (!clusterClient.Settings.SshIsConfigured)
        {
            return OperationResult.Fail("SSH not available for this cluster! Please configure SSH in the cluster settings.");
        }

        var client = await clusterClient.GetPveClientAsync();
        var script = string.Empty;
        var valid = false;
        var ret = string.Empty;

        switch (vmType)
        {
            case VmType.Qemu:
                var config = await client.Nodes[node].Qemu[vmId].Config.GetAsync();
                if (config.AgentEnabled)
                {
                    var ping = await client.Nodes[node].Qemu[vmId].Agent.Ping.Ping();
                    if (ping.IsSuccessStatusCode)
                    {
                        switch (config.VmOsType)
                        {
                            case VmOsType.Windows:
                                if (config.OsType is "win10" or "win11")
                                {
                                    script = scriptWindows;
                                    valid = true;
                                }
                                break;

                            case VmOsType.Linux:
                                script = scriptLinux;
                                valid = true;
                                break;

                            case VmOsType.Solaris:
                            case VmOsType.Other: break;
                            default: break;
                        }
                    }
                    else
                    {
                        ret = "Agent not responding!";
                    }
                }
                else
                {
                    ret = "Agent not enabled!";
                }
                break;

            case VmType.Lxc:
                script = scriptLinux;
                valid = true;
                break;

            default: throw new InvalidEnumArgumentException("VM type not supported!");
        }

        if (valid)
        {
            var pveCmd = vmType switch
            {
                VmType.Qemu => $"qm guest exec --pass-stdin 1 --timeout {timeoutMs / 1000}",
                VmType.Lxc => "pct exec",
                _ => throw new InvalidEnumArgumentException()
            };

            var result = await clusterClient.SshExecuteAsync(node, true, [$"{pveCmd} {vmId} -- {script}"]);
            var sshResult = result[0];
            var stdOut = sshResult.StdOut ?? string.Empty;

            if (sshResult.IsSuccess && !string.IsNullOrEmpty(stdOut))
            {
                if (vmType == VmType.Qemu)
                {
                    try
                    {
                        var decData = JsonSerializer.Deserialize<QemuExecResult>(stdOut);
                        if (decData?.ExitCode != 0 || !string.IsNullOrEmpty(decData?.ErrData))
                        {
                            ret = $"Error executing command: {decData?.ErrData}";
                            valid = false;
                        }
                        else
                        {
                            ret = decData?.OutData;
                        }
                    }
                    catch (Exception ex)
                    {
                        ret = $"Error parsing QemuExecResult: {stdOut}\n{ex.Message}";
                        valid = false;
                    }
                }
                else
                {
                    ret = stdOut;
                }
            }
            else
            {
                valid = false;
                ret = sshResult.StdErr ?? "Error executing command!";
            }
        }

        return valid
                ? OperationResult.Ok(ret ?? string.Empty)
                : OperationResult.Fail(ret ?? string.Empty);
    }

    private class QemuExecResult
    {
        [JsonPropertyName("exitcode")]
        public int ExitCode { get; set; }

        [JsonPropertyName("exited")]
        public int Exited { get; set; }

        [JsonPropertyName("out-data")]
        public string OutData { get; set; } = default!;

        [JsonPropertyName("err-data")]
        public string? ErrData { get; set; }
    }
}
