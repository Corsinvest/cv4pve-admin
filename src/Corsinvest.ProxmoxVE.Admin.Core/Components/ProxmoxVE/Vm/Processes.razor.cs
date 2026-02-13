/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using System.Text.Json;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Vm;

public partial class Processes(IAdminService adminService) : IRefreshableData, IClusterName
{
    [EditorRequired, Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired, Parameter] public string ClusterName { get; set; } = default!;

    private IEnumerable<Data> Items { get; set; } = [];

    private class Data
    {
        public int PID { get; set; }
        public string Name { get; set; } = default!;
        public string Username { get; set; } = default!;
        public double CPU { get; set; }
        public long Memory { get; set; }
    }

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
    {
        var scriptLinux = """
            ps -eo pid,user,comm,%cpu,vsz,nlwp --no-headers | \
            awk '{printf("{\"PID\": %s, \"Username\": \"%s\", \"Name\": \"%s\", \"CPU\": %.2f, \"Memory\": %s},\n", $1, $2, $3, $4, $5*1024)}' | \
            sed '$s/,$//' | awk 'BEGIN { print "[" } { print } END { print "]" }'            
            """;

        var scriptWindows = """
            powershell.exe - <<'EOF'
            $ownerCache = @{}

            $cpu1 = Get-Process | Select-Object Id, ProcessName, CPU
            Start-Sleep -Seconds 1
            $cpu2 = Get-Process | Select-Object Id, ProcessName, CPU, PrivateMemorySize

            $result = @()
            foreach ($current in $cpu2) {
                $prev = $cpu1 | Where-Object { $_.Id -eq $current.Id }

                if ($prev -and $current.CPU -ne $null -and $prev.CPU -ne $null) {
                    $cpuDelta = $current.CPU - $prev.CPU
                    $percent = [math]::Round(($cpuDelta / 1) / 100 / [Environment]::ProcessorCount, 2)

                    if (-not $ownerCache.ContainsKey($current.Id)) {
                        try {
                            $ownerCache[$current.Id] = (Get-CimInstance Win32_Process -Filter "ProcessId=$($current.Id)").GetOwner().User
                        } catch {
                            $ownerCache[$current.Id] = "SYSTEM"
                        }
                    }

                    $result += [PSCustomObject]@{
                        Name     = $current.ProcessName
                        PID      = $current.Id
                        Username = $ownerCache[$current.Id]
                        CPU      = $percent
                        Memory   = $current.PrivateMemorySize
                    }
                }
            }

            $json = $result | ConvertTo-Json
            Write-Output $json            
            EOF
            """;

        Items = [];
        await InvokeAsync(StateHasChanged);

        var client = await adminService[ClusterName].GetPveClientAsync();
        var ret = await client.VmExecNativeAsync(Vm.Node, Vm.VmType, Vm.VmId, scriptLinux, scriptWindows);
        if (ret.IsSuccess) { Items = JsonSerializer.Deserialize<IEnumerable<Data>>(ret.Value)!; }
    }
}
