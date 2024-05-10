/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Humanizer.Bytes;
using Nextended.Core.Extensions;
using System.Globalization;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services.DiskInfo;

public class ZfsDiskInfo(string ipAddress, string path, long vmId, string disk, string host, string spaceName) 
    : DiskInfoBase(vmId, disk, host, spaceName, true)
{
    public string IpAddress { get; } = ipAddress;
    public string Path { get; } = path;
    public override string Type => "ZFS";

    public static async Task<IEnumerable<ZfsDiskInfo>> ReadAsync(PveClient client, ClusterOptions clusterOptions)
    {
        var ret = new List<ZfsDiskInfo>();

        var storages = (await client.Storage.GetAsync("zfspool")).Where(a => !a.Disable).ToList();
        if (storages.Count != 0)
        {
            var nodes = await client.GetResourcesAsync(Api.Shared.Models.Cluster.ClusterResourceType.Node);

            foreach (var (host, ipAddress) in await client.GetHostAndIpAsync())
            {
                if (nodes.FirstOrDefault(a => a.Node == host)!.IsOnline)
                {
                    var info = clusterOptions.GetNodeOptions(ipAddress, host);
                    if (info != null)
                    {
                        try
                        {
                            var (ExitCode, StdOut, Error) = SshHelper.Execute(clusterOptions,
                                                                              ipAddress,
                                                                              info.SshPort,
                                                                              ["zfs list -H -t snapshot"])
                                                                     .FirstOrDefault();
                            if (ExitCode == 0)
                            {
                                foreach (var item in StdOut.Split('\n').Where(a => !string.IsNullOrEmpty(a) && !string.IsNullOrWhiteSpace(a)))
                                {
                                    var fullPath = item.Split('\t');
                                    var fullDisk = fullPath[0].Split("@");
                                    var data = fullDisk[0].Split("/");

                                    var diskVm = data.Last();
                                    var spaceName = data[..^1].JoinAsString("/");

                                    var disk = new ZfsDiskInfo(ipAddress,
                                                               fullPath[0],
                                                               long.Parse(diskVm.Split('-')[1]),
                                                               diskVm,
                                                               host,
                                                               spaceName);

                                    //check if exists
                                    disk = ret.FirstOrDefault(a => a.Disk == disk.Disk && a.Host == disk.Host) ?? disk;

                                    if (fullDisk.Length == 2)
                                    {
                                        var size = ByteSize.Parse(fullPath[1]
                                                                    .EnsureEndsWith(ByteSize.ByteSymbol)
                                                                    .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator))
                                                           .Bytes;

                                        disk.Snapshots.Add(new DiskInfoSnapshot(fullDisk[1],
                                                                                     size,
                                                                                     fullDisk[1].StartsWith("__replicate_") && fullDisk[1].EndsWith("__")));
                                    }

                                    if (!ret.Contains(disk)) { ret.Add(disk); }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        return ret;
    }
}