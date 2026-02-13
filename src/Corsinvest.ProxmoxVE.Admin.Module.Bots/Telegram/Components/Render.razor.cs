/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Radzen;

namespace Corsinvest.ProxmoxVE.Admin.Module.Bots.Telegram.Components;

public partial class Render(IServiceScopeFactory serviceScopeFactory,
                            IBrowserService browserService,
                            DialogService dialogService) : IClusterName, IDisposable
{
    [CascadingParameter(Name = nameof(ClusterName))] public string ClusterName { get; set; } = default!;

    private BotgramService BotgramService { get; set; } = default!;
    private string Log { get; set; } = string.Empty;
    private string Message { get; set; } = default!;
    private long ChatId { get; set; } = default!;

    private record Data(long ChatId, string User);

    private IEnumerable<Data> Chats
    {
        get
        {
            var chats = new List<Data>
            {
                new(0, "All User")
            };

            if (BotgramService != null) { chats.AddRange(BotgramService.GetChats(ClusterName).Select(a => new Data(a.Key, a.Value))); }
            return chats;
        }
    }

    public void Dispose()
    {
        var log = BotgramService?.GetLog(ClusterName);
        if (log != null) { log.WritedData -= Log_WritedData; }
    }

    protected override void OnInitialized()
    {
        using var scope = serviceScopeFactory.CreateScope();
        BotgramService = scope.ServiceProvider.GetServices<IHostedService>()
                                              .OfType<BotgramService>()
                                              .FirstOrDefault()!;

        Log = string.Empty;
        var log = BotgramService.GetLog(ClusterName);
        if (log != null)
        {
            Log = log.ToString();
            log.WritedData += Log_WritedData;
        }
    }

    private void Log_WritedData(object? sender, string e)
    {
        Log += e;
        InvokeAsync(StateHasChanged);
    }

    private async Task KeyUpAsync(KeyboardEventArgs e)
    {
        if (e.Code is "Enter" or "NumpadEnter") { await SendMessageAsync(); }
    }

    private async Task SendMessageAsync()
    {
        if (!string.IsNullOrWhiteSpace(Message))
        {
            if (ChatId == 0)
            {
                foreach (var item in Chats.Where(a => a.ChatId > 0))
                {
                    await BotgramService.SendMessageAsync(ClusterName, item.ChatId, Message);
                }
            }
            else
            {
                await BotgramService.SendMessageAsync(ClusterName, ChatId, Message);
            }
            Message = string.Empty;
        }
    }
}
