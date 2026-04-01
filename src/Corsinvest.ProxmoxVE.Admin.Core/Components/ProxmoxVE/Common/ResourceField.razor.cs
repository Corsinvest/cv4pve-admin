/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Api.Shared.Models.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public partial class ResourceField(DialogService DialogService)
{
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public ResourceFieldType ResourceType { get; set; } = ResourceFieldType.Guest;
    [Parameter] public bool UseJolly { get; set; } = true;
    [Parameter] public string ClusterName { get; set; } = string.Empty;
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public bool ReadOnly { get; set; }
    [Parameter] public bool MultipleSelection { get; set; } = true;
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private async Task OnChipRemoved(object? chip)
    {
        if (chip is string removed)
        {
            var chips = Chips.Where(c => c != removed);
            await ValueChanged.InvokeAsync(string.Join(",", chips));
        }
    }

    private IEnumerable<string> Chips => string.IsNullOrWhiteSpace(Value)
        ? []
        : Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

    private async Task OpenPickerAsync()
    {
        if (ResourceType == ResourceFieldType.Guest && UseJolly)
        {
            var ret = await DialogService.OpenSideExAsync<VmJollyPickerDialog>(L["Select VM/CT"],
                                                                               new()
                                                                               {
                                                                                   [nameof(VmJollyPickerDialog.ClusterName)] = ClusterName,
                                                                                   [nameof(VmJollyPickerDialog.Value)] = Value ?? string.Empty,
                                                                               },
                                                                               new()
                                                                               {
                                                                                   CloseDialogOnOverlayClick = true,
                                                                                   Resizable = true,
                                                                                   Width = "900px",
                                                                               });

            if (ret is string result) { await ValueChanged.InvokeAsync(result); }
        }
        else
        {
            var clusterResourceType = ResourceType switch
            {
                ResourceFieldType.Node => ClusterResourceType.Node,
                ResourceFieldType.Storage => ClusterResourceType.Storage,
                _ => ClusterResourceType.Vm,
            };

            var dataGridSettings = ResourceType switch
            {
                ResourceFieldType.Node => RadzenHelper.MakeDataGridSettings(
                [
                    nameof(ClusterResourceItem.Status),
                    nameof(ClusterResourceItem.Description),
                    nameof(ClusterResourceItem.CpuInfo),
                    nameof(ClusterResourceItem.MemoryInfo),
                ]),
                ResourceFieldType.Storage => RadzenHelper.MakeDataGridSettings(
                [
                    nameof(ClusterResourceItem.Status),
                    nameof(ClusterResourceItem.Node),
                    nameof(ClusterResourceItem.Description),
                    nameof(ClusterResourceItem.DiskSize),
                    nameof(ClusterResourceItem.DiskUsage),
                ]),
                ResourceFieldType.Guest => RadzenHelper.MakeDataGridSettings(
                [
                    nameof(ClusterResourceItem.Status),
                    nameof(ClusterResourceItem.Type),
                    nameof(ClusterResourceItem.Node),
                    nameof(ClusterResourceItem.Description),
                    nameof(ClusterResourceItem.CpuInfo),
                    nameof(ClusterResourceItem.MemoryInfo),
                ]),
                _ => throw new NotImplementedException(),
            };

            var ret = await DialogService.OpenSideExAsync<ResourcesPickerDialog>(L["Select"],
                                                                                   new()
                                                                                   {
                                                                                       [nameof(ResourcesPickerDialog.ClusterNames)] = (IEnumerable<string>)[ClusterName],
                                                                                       [nameof(ResourcesPickerDialog.ResourceType)] = clusterResourceType,
                                                                                       [nameof(ResourcesPickerDialog.FilterExpression)] = (Func<ClusterResourceItem, string, bool>)((a, _) => a.ResourceType == clusterResourceType),
                                                                                       [nameof(ResourcesPickerDialog.SelectionMode)] = MultipleSelection
                                                                                                                                            ? DataGridSelectionMode.Multiple
                                                                                                                                            : DataGridSelectionMode.Single,

                                                                                       [nameof(ResourcesPickerDialog.AvailableCommands)] = false,
                                                                                       [nameof(ResourcesPickerDialog.ShowSnapshotSize)] = false,
                                                                                       [nameof(ResourcesPickerDialog.DescriptionAsLink)] = false,
                                                                                       [nameof(ResourcesPickerDialog.DataGridSettings)] = dataGridSettings,
                                                                                       [nameof(ResourcesPickerDialog.PropertyIconStatus)] = ResourcesViewPropertyIconStatus.Status,
                                                                                       [nameof(ResourcesPickerDialog.IconStatus)] = ResourceColumnIconStatus.IconAndText,
                                                                                   },
                                                                                   new()
                                                                                   {
                                                                                       CloseDialogOnOverlayClick = true,
                                                                                       Resizable = true,
                                                                                       Width = "700px",
                                                                                   });

            if (ret is ClusterResourceItem[] selected && selected.Length > 0)
            {
                var data = ResourceType switch
                {
                    ResourceFieldType.Node => selected.Select(r => r.Node),
                    ResourceFieldType.Storage => selected.Select(r => r.Id),
                    ResourceFieldType.Guest => selected.Select(r => r.VmId.ToString()),
                    _ => throw new NotImplementedException(),
                };

                await ValueChanged.InvokeAsync(string.Join(",", data));
            }
        }
    }
}
