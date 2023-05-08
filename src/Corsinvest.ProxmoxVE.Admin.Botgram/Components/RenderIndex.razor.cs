/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.Botgram.Components;

public partial class RenderIndex : IDisposable
{
    [Inject] private IServiceScopeFactory ServiceScopeFactory { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private BotgramService BotgramService { get; set; } = default!;
    private string Log { get; set; } = string.Empty;
    private string Message { get; set; } = default!;
    private long ChatId { get; set; } = default!;
    private string _clusterName = default!;

    private IReadOnlyDictionary<long, string> Chats
    {
        get
        {
            var ret = new Dictionary<long, string>() { { 0, "All User" } };

            if (BotgramService != null)
            {
                ret = ret.Union(BotgramService.GetChats(_clusterName))
                         .ToDictionary(a => a.Key, a => a.Value);
            }

            return ret;
        }
    }

    public void Dispose()
    {
        var log = BotgramService?.GetLog(_clusterName);
        if (log != null) { log.WritedData -= Log_WritedData; }
    }

    protected override async Task OnInitializedAsync()
    {
        _clusterName = await PveClientService.GetCurrentClusterName();

        var scope = ServiceScopeFactory.CreateScope();
        BotgramService = scope.ServiceProvider.GetServices<IHostedService>()
                                              .OfType<BotgramService>()
                                              .FirstOrDefault()!;

        Log = "";
        var log = BotgramService.GetLog(_clusterName);
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
        if (e.Code == "Enter" || e.Code == "NumpadEnter") { await SendMaessageAsync(); }
    }

    private async Task SendMaessageAsync()
    {
        if (!string.IsNullOrWhiteSpace(Message))
        {
            if (ChatId == 0)
            {
                foreach (var item in Chats.Keys.Where(a => a > 0))
                {
                    await BotgramService.SendMessageAsync(_clusterName, item, Message);
                }
            }
            else
            {
                await BotgramService.SendMessageAsync(_clusterName, ChatId, Message);
            }
            Message = "";
        }
    }
}