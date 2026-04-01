/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components.ProxmoxVE.Common;

public partial class VmJollyPickerDialog(IAdminService adminService, DialogService dialogService) : IClusterName
{
    [Parameter] public string ClusterName { get; set; } = string.Empty;
    [Parameter] public string Value { get; set; } = string.Empty;

    private ResourcesView? ResourcesExRef { get; set; }
    private HashSet<long> PreviewVmIds { get; set; } = [];
    private DataGridSettings DataGridSettings { get; set; } = new();
    private List<string> Tokens { get; set; } = [];
    private List<string> AvailableTokens { get; set; } = [];
    private string CurrentToken { get; set; } = string.Empty;
    private int AutoCompleteKey { get; set; }

    protected override void OnInitialized()
        => RadzenHelper.MakeDataGridSettings(DataGridSettings,
                                             [nameof(ClusterResourceItem.Status),
                                              nameof(ClusterResourceItem.Type),
                                              nameof(ClusterResourceItem.Node),
                                              nameof(ClusterResourceItem.Description),
                                              nameof(ClusterResourceItem.CpuInfo),
                                              nameof(ClusterResourceItem.MemoryInfo),]);

    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrWhiteSpace(Value))
        {
            Tokens = [.. Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim())];
        }

        var client = await adminService[ClusterName].GetPveClientAsync();
        AvailableTokens = [.. await client.GetVmIdsAsync(true, true, true, true, true, true)];

        await RefreshPreviewAsync();
    }

    private async Task OnAutoCompleteChange(object value)
    {
        if (value is string s) { CurrentToken = s; }
        await AddToken();
    }

    private async Task AutoCompleteKeyUp(KeyboardEventArgs e)
    {
        if (e.Key == "Enter") { await AddToken(); }
    }

    private async Task AddToken()
    {
        var token = CurrentToken.Trim();
        if (string.IsNullOrEmpty(token)) { return; }
        if (!Tokens.Contains(token))
        {
            Tokens.Add(token);
            await RefreshPreviewAsync();
        }
        CurrentToken = string.Empty;
        AutoCompleteKey++;
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
            var vms = await client.GetVmsAsync(string.Join(",", Tokens));
            PreviewVmIds = [.. vms.Select(v => v.VmId)];
        }
        finally
        {
            await InvokeAsync(StateHasChanged);
            if (ResourcesExRef != null) { await ResourcesExRef.RefreshDataAsync(); }
        }
    }

    private bool FilterPreview(ClusterResourceItem item, string _) => PreviewVmIds.Contains(item.VmId);
    private void OnSelect() => dialogService.Close(string.Join(",", Tokens));
}
