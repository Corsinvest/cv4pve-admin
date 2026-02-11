using System.Text.Json;
using System.Text.Json.Serialization;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using OperationResult = FluentResults.Result;

namespace Corsinvest.ProxmoxVE.Admin.Core.Extensions;

public static class PveClientExtensions
{
    public static async Task<bool> CheckClusterIsValidVersionAsync(this PveClient client)
        => Version.TryParse((await client.Version.GetAsync()).Version.Split("-")[0], out var version)
            && version >= PveAdminHelper.MinimalVersion;

    public static async Task<FluentResults.Result<string>> VmExecNativeAsync(this PveClient client,
                                                                             string node,
                                                                             VmType vmType,
                                                                             long vmId,
                                                                             string scriptLinux,
                                                                             string scriptWindows,
                                                                             int timeoutMs = 120000)
    {
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

                            case VmOsType.Solaris: break;
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

            await using var webTerm = new PveWebTermClient(client, node);
            await webTerm.ConnectAsync();
            var (stdOut, stdErr, ExitCode) = await webTerm.ExecuteCommandAsync($"{pveCmd} {vmId} -- {script}", timeoutMs);
            var result = stdOut ?? string.Empty;
            if (ExitCode == 0 && !string.IsNullOrEmpty(result))
            {
                if (vmType == VmType.Qemu)
                {
                    try
                    {
                        var decData = JsonSerializer.Deserialize<QemuExecResult>(result);
                        if (decData?.ExitCode != 0 || !string.IsNullOrEmpty(decData?.ErrData))
                        {
                            result = $"Error executing command: {decData?.ErrData}";
                            valid = false;
                        }
                        else
                        {
                            result = decData?.OutData;
                        }
                    }
                    catch (Exception ex)
                    {
                        result = $"Error parsing QemuExecResult: {result}\n{ex.Message}";
                        valid = false;
                    }
                }
            }
            else
            {
                valid = false;
                result = stdErr ?? "Error exeguting command!";
            }

            ret = result;
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
