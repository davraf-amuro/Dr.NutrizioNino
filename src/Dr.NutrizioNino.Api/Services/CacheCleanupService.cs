using Dr.NutrizioNino.Api.Infrastructure;

namespace Dr.NutrizioNino.Api.Services;

/// <summary>Hosted service that purges expired NutrientExtractionCache rows on startup and every 24 hours.</summary>
public class CacheCleanupService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<CacheCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan _interval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunCleanupAsync(stoppingToken);
        using var timer = new PeriodicTimer(_interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunCleanupAsync(stoppingToken);
        }
    }

    /// <summary>Resolves a scoped DrRepository, computes the cutoff from config, and deletes expired rows.</summary>
    private async Task RunCleanupAsync(CancellationToken ct)
    {
        var ttlMinutes = configuration.GetValue<int>("Vision:Cache:TtlMinutes", 1439);
        var cutoff = DateTime.UtcNow.AddMinutes(-ttlMinutes);
        await using var scope = scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<DrRepository>();
        var deleted = await repo.DeleteExpiredCacheAsync(cutoff, ct);
        logger.LogInformation("Cache cleanup: {Deleted} record eliminati (ttl={Ttl}m, cutoff={Cutoff:O})",
            deleted, ttlMinutes, cutoff);
    }
}
