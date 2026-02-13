/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Components.SendMessage;
using Corsinvest.ProxmoxVE.Admin.Core.Session;

namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class ActiveSessions(ISessionsInfoTracker sessionsInfoTracker,
                                    DialogService dialogService,
                                    ISessionHubService sessionHubService) : IRefreshableData
{
    [Parameter] public string? UserName { get; set; }

    private IEnumerable<SessionInfo> Items { get; set; } = [];
    private IList<SessionInfo> SelectedItems { get; set; } = [];
    private RadzenDataGrid<SessionInfo> DataGridRef { get; set; } = default!;

    protected override async Task OnInitializedAsync() => await RefreshDataAsync();

    public async Task RefreshDataAsync()
        => Items = sessionsInfoTracker.Sessions.Where(a => a.UserName == UserName, !string.IsNullOrEmpty(UserName));

    private async Task LogoutAsync()
    {
        if (await dialogService.ConfirmAsync(L["Are you sure?"], L["Logout session"], true))
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
    {
        var result = await dialogService.OpenSideAsync<SendMessageDialog>(L["Send Message"]);
        if (result is SendMessageModel messageModel)
        {
            foreach (var item in SelectedItems.Where(a => !string.IsNullOrEmpty(a.HubConnectionId)))
            {
                await sessionHubService.SendMessageAsync(item.HubConnectionId!, messageModel.Message, messageModel.Title, messageModel.Severity);
            }

            SelectedItems.Clear();
            await DataGridRef.Reload();
        }
    }
}
