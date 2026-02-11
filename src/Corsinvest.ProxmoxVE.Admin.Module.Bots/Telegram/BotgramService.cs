using Corsinvest.ProxmoxVE.TelegramBot.Api;

namespace Corsinvest.ProxmoxVE.Admin.Module.Bots.Telegram;

public class BotgramService(ILogger<BotgramService> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly ILogger _logger = logger;

    private List<BotData> BotDatas { get; } = [];

    private class BotData : IClusterName
    {
        public BotManager BotManager { get; set; } = default!;
        public StringWriterEvent Log { get; set; } = default!;
        public string ClusterName { get; set; } = default!;
        public DateTime LastUpdate { get; set; }
    }

    public async Task SendMessageAsync(string clusterName, long chatId, string message)
    {
        var botData = BotDatas.FromClusterName(clusterName);
        if (botData != null) { await botData.BotManager.SendMessageAsync(chatId, message); }
    }

    public DateTime? GetLastUpdate(string clusterName) => BotDatas.FromClusterName(clusterName)?.LastUpdate;
    public IReadOnlyDictionary<long, string> GetChats(string clusterName) => BotDatas.FromClusterName(clusterName)?.BotManager.Chats ?? new Dictionary<long, string>();
    public StringWriterEvent? GetLog(string clusterName) => BotDatas.FromClusterName(clusterName)?.Log;

    public async Task RestartAsync()
    {
        await StopAsync(new CancellationToken());
        await StartAsync(new CancellationToken());
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();
        scope.GetLoggerFactory();

        foreach (var item in scope.GetAdminService())
        {
            var clusterName = item.Settings.Name;

            var settings = scope.GetSettingsService().GetForModule<Module, Settings>(clusterName);
            if (settings.Telegram.Enabled)
            {
                var botData = new BotData
                {
                    ClusterName = clusterName,
                    Log = new StringWriterEvent()
                };
                botData.Log.WritedData += Log_WritedData;

                _logger.LogInformation("Botgram service is starting... Cluster {clusterName}", clusterName);

                try
                {
                    botData.BotManager = new BotManager(item.GetPveClientAsync,
                                                        settings.Telegram.Token,
                                                        settings.Telegram.GetChatsId(),
                                                        botData.Log);

                    await botData.BotManager.StartReceiving();
                    botData.Log.WriteLine($"Started bot in cluster {clusterName}...");
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
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bot service execution loop was interrupted");
        }

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
    }

    private void Log_WritedData(object? sender, string e)
    {
        var botData = BotDatas.FirstOrDefault(a => a.Log == sender);
        botData?.LastUpdate = DateTime.Now;
    }
}
