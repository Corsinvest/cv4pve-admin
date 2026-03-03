/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class ActiveSessions(ISessionsInfoTracker sessionsInfoTracker,
                                    IServiceProvider serviceProvider,
                                    DialogService dialogService) : IRefreshableData
{
    [Parameter] public string? UserName { get; set; }

    private IEnumerable<SessionInfo> Items { get; set; } = [];
    private IList<SessionInfo> SelectedItems { get; set; } = [];
    private RadzenDataGrid<SessionInfo> DataGridRef { get; set; } = default!;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public Task RefreshDataAsync()
    {
        Items = sessionsInfoTracker.Sessions.Where(a => a.UserName == UserName, !string.IsNullOrEmpty(UserName));
        return Task.CompletedTask;
    }

    private async Task LogoutAsync()
    {
        var sessionHubService = serviceProvider.GetService<ISessionHubService>();
        if (sessionHubService != null
            && await dialogService.ConfirmAsync(L["Are you sure?"], L["Logout session"], true))
        {
            foreach (var item in SelectedItems.Where(a => !string.IsNullOrEmpty(a.HubConnectionId)))
            {
                await sessionHubService.ForceLogoutAsync(item.HubConnectionId!, L["Your session has been terminated by an administrator"]);
            }

            SelectedItems.Clear();
            await DataGridRef.Reload();
        }
    }

    private async Task SendMessageAsync()
        => await dialogService.OpenSideExAsync<SendMessageDialog>(L["Send Message"],
                                                                  new()
                                                                  {
                                                                      [nameof(SendMessageDialog.HubConnectionIds)] = SelectedItems.Where(a => !string.IsNullOrEmpty(a.HubConnectionId))
                                                                                                                                .Select(a => a.HubConnectionId!)
                                                                                                                                .ToList()
                                                                  },
                                                                  new());
}
