/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.Botgram.Components;

public partial class RenderWidget
{
    [Inject] private IServiceScopeFactory ServiceScopeFactory { get; set; } = default!;
    [Inject] private IPveClientService PveClientService { get; set; } = default!;

    private int Count { get; set; }
    private DateTime? LastUpdate { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var scope = ServiceScopeFactory.CreateScope();
        var botgramService = scope.ServiceProvider.GetServices<IHostedService>()
                                                  .OfType<BotgramService>()
                                                  .FirstOrDefault();

        var clusterName = await PveClientService.GetCurrentClusterName();

        Count = botgramService?.GetChats(clusterName).Count ?? 0;
        LastUpdate = botgramService?.GetLastUpdate(clusterName);
    }
}