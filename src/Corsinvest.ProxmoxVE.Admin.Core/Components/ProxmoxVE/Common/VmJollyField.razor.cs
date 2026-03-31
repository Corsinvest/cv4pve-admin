/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public partial class VmJollyField(DialogService dialogService)
{
    private async Task OpenPickerAsync()
    {
        var ret = await dialogService.OpenSideExAsync<VmJollyPickerDialog>(L["Select VM/CT"],
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
}
