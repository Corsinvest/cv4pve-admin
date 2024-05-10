/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.AppHero.Core.Service;
using Corsinvest.ProxmoxVE.Api;
using Corsinvest.ProxmoxVE.Api.Extension.Utils;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Common;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Node;
using Corsinvest.ProxmoxVE.Api.Shared.Models.Vm;
using Corsinvest.ProxmoxVE.Api.Shared.Utils;

namespace Corsinvest.ProxmoxVE.Admin.Core.UI.ProxmoxVE.Vm;

public partial class VmManager
{
    [EditorRequired][Parameter] public IClusterResourceVm Vm { get; set; } = default!;
    [EditorRequired][Parameter] public PveClient PveClient { get; set; } = default!;
    [Parameter] public bool ShowDetailProxmoxVE { get; set; }
    [Parameter] public Func<string>? GetUrlShowConsole { get; set; }
    [Parameter] public bool CanRestoreFileBackup { get; set; }
    [Parameter] public bool CanCreateSnapshot { get; set; }
    [Parameter] public bool CanDeleteSnapshot { get; set; }
    [Parameter] public bool CanRollbackSnapshot { get; set; }
    [Parameter] public bool CanNoVnc { get; set; }
    [Parameter] public bool CanChangeStatus { get; set; }
    [Parameter] public Func<NodeStorageContent, NodeBackupFile, string> GetUrlRestoreFile { get; set; } = default!;

    [Inject] private IBrowserService BrowserService { get; set; } = default!;

    private int ActivePanelIndex { get; set; }
    private Detail? RefDetail { get; set; } = default!;
    private Charts? RefCharts { get; set; } = default!;
    private IEnumerable<string> PropertiesNameTasks { get; set; } = [];

    protected override void OnInitialized()
    {
        var fieldsTasks = new[]
        {
            nameof(NodeTask.StartTimeDate),
            nameof(NodeTask.EndTimeDate),
            nameof(NodeTask.DurationInfo),
            nameof(NodeTask.DescriptionFull),
            nameof(NodeTask.Status),
        }.ToList();
        fieldsTasks.AddIf(ShowDetailProxmoxVE, nameof(NodeTask.User));
        PropertiesNameTasks = fieldsTasks;
    }

    private async Task<VmBaseStatusCurrent> GetStatusAsync()
    {
        //StateHasChanged();
        return await PveClient.GetVmStatusAsync(Vm);
    }

    private async Task Refresh()
    {
        switch (ActivePanelIndex)
        {
            case 0: await RefDetail!.Refresh(); break;
            case 1: await RefCharts!.Refresh(); break;
            default: break;
        }
    }

    private async Task<VmQemuAgentGetFsInfo> GetQemuAgentGetFsInfoAsync()
    {
        VmQemuAgentGetFsInfo ret = null!;
        try
        {
            ret = await PveClient.Nodes[Vm.Node].Qemu[Vm.VmId].Agent.GetFsinfo.GetAsync();
        }
        catch { }

        return ret;
    }

    private async Task ChangeStatusAsync(VmStatus status)
    {
        await VmHelper.ChangeStatusVmAsync(PveClient, Vm.Node, Vm.VmType, Vm.VmId, status);

        //todo salavare log su db
        //await DbHelper.SaveLogAction(Db,
        //                             vm.VmId,
        //                             AuthenticationState.User.Identity.Name,
        //                             "CHANGE STATUS",
        //                             $"Change status VM to {status}");
    }

    private async Task ShowConsoleAsync()
    {
        var url = GetUrlShowConsole == null
                    ? NoVncHelper.GetConsoleUrl(PveClient.Host,
                                                PveClient.Port,
                                                NoVncHelper.GetConsoleType(Vm.VmType),
                                                Vm.Node,
                                                Vm.VmId,
                                                Vm.Name)
                    : GetUrlShowConsole.Invoke();

        await BrowserService.OpenAsync(url, "_blank");
    }

    private async Task<IEnumerable<VmRrdData>> GetRrdDataAsync(RrdDataTimeFrame rrdDataTimeFrame, RrdDataConsolidation rrdDataConsolidation)
        => await PveClient.GetVmRrdDataAsync(Vm, rrdDataTimeFrame, rrdDataConsolidation);

    private async Task<IEnumerable<NodeTask>> GetTasks()
        => await PveClient.Nodes[Vm.Node].Tasks.GetAsync(start: 0, limit: 500, vmid: Convert.ToInt32(Vm.VmId));
}