/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api;
using Newtonsoft.Json;
using Renci.SshNet;

namespace Corsinvest.ProxmoxVE.Admin.Core.Services.DiskInfo;

public class CephDiskInfo(string storage, string pool, long vmId, string disk, string monitorHosts)
    : DiskInfoBase(vmId, disk, monitorHosts, pool, false)
{
    public string Storage { get; } = storage;
    public string Pool { get; } = pool;
    public string MonitorHosts { get; } = monitorHosts;
    public override string Type => "Ceph";

    private class ImageSnapshot
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = default!;

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("protected")]
        public bool Protected { get; set; }

        //[JsonProperty("timestamp")]
        //public DateTime Timestamp { get; set; }
    }

    public static async Task<IEnumerable<CephDiskInfo>> ReadAsync(PveClient client, ClusterOptions clusterOptions)
    {
        var ret = new List<CephDiskInfo>();

        var (host, ipAddress) = (await client.GetHostAndIpAsync()).FirstOrDefault(a => a.Value == client.Host);
        var info = clusterOptions.GetNodeOptions(ipAddress, host);
        if (info != null)
        {
            var storages = (await client.Storage.GetAsync("rbd")).Where(a => !a.Disable).ToList();
            if (storages.Count != 0)
            {
                using var sshClient = new SshClient(ipAddress,
                                                    info.SshPort,
                                                    clusterOptions.SshCredential.Username,
                                                    clusterOptions.SshCredential.Password);
                sshClient.Connect();

                IEnumerable<(int ExitCode, string StdOut, string Error)> ExecuteSsh(IEnumerable<string> commands)
                {
                    var ret = new List<(int ExitCode, string StdOut, string Error)>();
                    foreach (var command in commands)
                    {
                        using var cmd = sshClient.CreateCommand(command);
                        ret.Add((Convert.ToInt32(cmd.ExitStatus), cmd.Execute(), cmd.Error));
                    }
                    return ret;
                }

                foreach (var item in storages)
                {
                    var cmdBase = "rbd";
                    if (!string.IsNullOrEmpty(item.Username)) { cmdBase += $" --id {item.Username}"; }
                    if (!string.IsNullOrEmpty(item.Monhost)) { cmdBase += $" -m {item.Monhost}"; }
                    if (!string.IsNullOrEmpty(item.Storage)) { cmdBase += $" --keyring /etc/pve/priv/ceph/{item.Storage}.keyring"; }

                    var (ExitCode, StdOut, Error) = ExecuteSsh([$"{cmdBase} ls {item.Pool}"]).FirstOrDefault();
                    if (ExitCode == 0)
                    {
                        var images = StdOut.Split('\n').Where(a => !string.IsNullOrEmpty(a)).ToArray();
                        var retSnaps = ExecuteSsh([.. images.Select(a => $"{cmdBase} snap ls {item.Pool}/{a} --format json")])
                                            .ToList();

                        for (var i = 0; i < images.Length; i++)
                        {
                            if (retSnaps[i].ExitCode == 0)
                            {
                                var disk = new CephDiskInfo(item.Storage,
                                                            item.Pool,
                                                            long.Parse(images[i].Split('-')[1]),
                                                            images[i],
                                                            item.Monhost);

                                //check if exists
                                disk = ret.FirstOrDefault(a => a.Disk == disk.Disk) ?? disk;

                                var imgSnapshots = JsonConvert.DeserializeObject<List<ImageSnapshot>>(retSnaps[i].StdOut)!;
                                disk.Snapshots.AddRange(imgSnapshots.Select(a => new DiskInfoSnapshot(a.Name, a.Size, false)));

                                if (!ret.Contains(disk)) { ret.Add(disk); }
                            }
                        }
                    }
                }
                sshClient.Disconnect();
            }
        }

        return ret;
    }
}
