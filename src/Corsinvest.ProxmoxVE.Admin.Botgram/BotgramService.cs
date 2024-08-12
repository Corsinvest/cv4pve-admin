/*
 * SPDX-FileCopyrightText: Copyright Corsinvest Srl
 * SPDX-License-Identifier: AGPL-3.0-only
 */
using Corsinvest.ProxmoxVE.Admin.Core.Extensions;
using Corsinvest.ProxmoxVE.Admin.Core.Repository;
using Corsinvest.ProxmoxVE.TelegramBot.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Corsinvest.ProxmoxVE.Admin.Botgram;

public class BotgramService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private List<BotData> BotDatas { get; } = [];

    private class BotData : IClusterName
    {
        public BotManager BotManager { get; set; } = default!;
        public StringWriterEvent Log { get; set; } = default!;
        public string ClusterName { get; set; } = default!;
        public DateTime LastUpdate { get; set; }
    }

    public BotgramService(ILogger<BotgramService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task SendMessageAsync(string clusterName, long chatId, string message)
    {
        var botData = BotDatas.IsCluster(clusterName);
        if (botData != null) { await botData.BotManager.SendMessageAsync(chatId, message); }
    }

    public DateTime? GetLastUpdate(string clusterName) => BotDatas.IsCluster(clusterName)?.LastUpdate;
    public IReadOnlyDictionary<long, string> GetChats(string clusterName) => BotDatas.IsCluster(clusterName)?.BotManager.Chats ?? new Dictionary<long, string>();
    public StringWriterEvent? GetLog(string clusterName) => BotDatas.IsCluster(clusterName)?.Log;

    public async Task RestartAsync()
    {
        await StopAsync(new CancellationToken());
        await StartAsync(new CancellationToken());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var pveClientService = scope.GetPveClientService();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var options = scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<Options>>().Value;

        foreach (var item in options.Clusters)
        {
            if (item.Enabled)
            {
                var botData = new BotData
                {
                    ClusterName = item.ClusterName,
                    Log = new StringWriterEvent()
                };
                botData.Log.WritedData += Log_WritedData;

                _logger.LogInformation("Botgram service is starting... Cluster {clusterName}", item.ClusterName);
                var clusterOptions = pveClientService.GetClusterOptions(item.ClusterName)!;

                try
                {
                    botData.BotManager = new BotManager(clusterOptions.ApiHostsAndPortHA,
                                                        clusterOptions.ApiToken,
                                                        clusterOptions.ApiCredential.Username,
                                                        clusterOptions.ApiCredential.Password,
                                                        clusterOptions.VerifyCertificate,
                                                        loggerFactory,
                                                        item.Token,
                                                        item.GetChatsId(),
                                                        botData.Log);
                    botData.BotManager.StartReceiving();
                    botData.Log.WriteLine($"Started bot in cluster {item.ClusterName}...");
                    BotDatas.Add(botData);
                }
                catch (Exception ex) { _logger.LogError(ex, ex.Message); }
            }
        }

        try
        {
            while (!stoppingToken.IsCancellationRequested && BotDatas.Count != 0)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        catch { }

        foreach (var item in BotDatas)
        {
            try
            {
                item.BotManager.StopReceiving();
                item.Log.WriteLine($"End bot in cluster {item.ClusterName}...");
                item.Log.WritedData -= Log_WritedData;
            }
            catch (Exception ex) { _logger.LogError(ex, ex.Message); }
        }

        BotDatas.Clear();

        await Task.CompletedTask;
    }

    private void Log_WritedData(object? sender, string e)
    {
        var botData = BotDatas.FirstOrDefault(a => a.Log == sender);
        if (botData != null) { botData.LastUpdate = DateTime.Now; }
    }
}
