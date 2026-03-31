/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Cluster;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public partial class VmJollyPickerDialog(IAdminService adminService, DialogService dialogService)
{
    [Parameter] public string ClusterName { get; set; } = string.Empty;
    [Parameter] public string Value { get; set; } = string.Empty;

    private ResourcesEx? ResourcesExRef { get; set; }
    private HashSet<long> PreviewVmIds { get; set; } = [];
    private DataGridSettings DataGridSettings { get; set; } = new();
    private List<string> Tokens { get; set; } = [];
    private List<string> AvailableTokens { get; set; } = [];
    private string? SelectedPreset { get; set; }
    private string CustomToken { get; set; } = string.Empty;

    protected override void OnInitialized()
        => RadzenHelper.MakeDataGridSettings(DataGridSettings,
                                             [nameof(ClusterResourceEx.Status),
                                              nameof(ClusterResourceEx.Type),
                                              nameof(ClusterResourceEx.Node),
                                              nameof(ClusterResourceEx.Description)]);

    protected override async Task OnInitializedAsync()
    {
        // Parse input value into tokens
        if (!string.IsNullOrWhiteSpace(Value))
        {
            Tokens = [.. Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim())];
        }

        // Load available tokens for autocomplete
        var client = await adminService[ClusterName].GetPveClientAsync();
        AvailableTokens = [.. await client.GetVmIdsAsync(true, true, true, true, true, true)];

        await RefreshPreviewAsync();
    }

    private async Task AddPreset()
    {
        if (string.IsNullOrEmpty(SelectedPreset)) { return; }
        if (!Tokens.Contains(SelectedPreset))
        {
            Tokens.Add(SelectedPreset);
            await RefreshPreviewAsync();
        }
        SelectedPreset = null;
    }

    private async Task AddCustom()
    {
        var token = CustomToken.Trim();
        if (string.IsNullOrEmpty(token)) { return; }
        if (!Tokens.Contains(token))
        {
            Tokens.Add(token);
            await RefreshPreviewAsync();
        }
        CustomToken = string.Empty;
    }

    private async Task RemoveToken(string token)
    {
        Tokens.Remove(token);
        await RefreshPreviewAsync();
    }

    private async Task RefreshPreviewAsync()
    {
        if (Tokens.Count == 0)
        {
            PreviewVmIds = [];
            await InvokeAsync(StateHasChanged);
        }

        await InvokeAsync(StateHasChanged);

        try
        {
            var client = await adminService[ClusterName].GetPveClientAsync();
            var jolly = string.Join(",", Tokens);
            var vms = await client.GetVmsAsync(jolly);
            PreviewVmIds = [.. vms.Select(v => v.VmId)];
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
            if (ResourcesExRef != null) { await ResourcesExRef.RefreshDataAsync(); }
        }
    }

    private bool FilterPreview(ClusterResourceEx item, string _) => PreviewVmIds.Contains(item.VmId);

    private void OnSelect() => dialogService.Close(string.Join(",", Tokens));
}
