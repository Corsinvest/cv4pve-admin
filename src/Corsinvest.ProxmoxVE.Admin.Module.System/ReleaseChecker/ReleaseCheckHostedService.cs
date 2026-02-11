using Microsoft.Extensions.Hosting;

namespace Corsinvest.ProxmoxVE.Admin.Module.System.ReleaseChecker;

public class ReleaseCheckHostedService(ILogger<ReleaseCheckHostedService> logger,
                                      IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12);
    private readonly TimeSpan _retryInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Release Check Service started");

        await CheckForUpdatesAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
                await CheckForUpdatesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Release Check Service stopping");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in Release check loop");

                try
                {
                    await Task.Delay(_retryInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }

    private async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var releaseService = scope.ServiceProvider.GetRequiredService<IReleaseService>();

        try
        {
            logger.LogDebug("Checking for new releases");

            var newRelease = await releaseService.NewReleaseIsAvailableAsync(includePrerelease: false, force: false, cancellationToken);

            if (newRelease != null)
            {
                logger.LogInformation("New release available: {Version} (published: {PublishedAt})",
                    newRelease.Version,
                    newRelease.PublishedAt);
            }
            else
            {
                logger.LogDebug("No new release available");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during release check");
        }
    }
}
