/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
namespace Corsinvest.ProxmoxVE.Admin.Core.Components;

public partial class SendMessageDialog(DialogService dialogService, IServiceProvider serviceProvider)
{
    [Parameter] public IEnumerable<string> HubConnectionIds { get; set; } = [];

    private SendMessageModel Model { get; set; } = new();

    private class SendMessageModel
    {
        [Required] public string Title { get; set; } = default!;
        [Required] public string Message { get; set; } = default!;
        public MessageSeverity Severity { get; set; } = MessageSeverity.Info;
    }

    private async Task SendMessageAsync()
    {
        var sessionHubService = serviceProvider.GetService<ISessionHubService>();
        if (sessionHubService != null)
        {
            foreach (var item in HubConnectionIds)
            {
                await sessionHubService.SendMessageAsync(item, Model.Message, Model.Title, Model.Severity);
            }
        }

        dialogService.Close();
    }
}
