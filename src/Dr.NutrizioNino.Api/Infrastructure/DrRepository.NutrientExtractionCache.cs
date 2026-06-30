using Dr.NutrizioNino.Api.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    /// <summary>Returns cached extraction for the given image hash + provider combination.</summary>
    public async Task<NutrientExtractionCache?> GetCacheByHashAsync(string imageHash, string providerKey, CancellationToken ct = default) =>
        await drContext.NutrientExtractionCache
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ImageHash == imageHash && c.ProviderKey == providerKey, ct)
            .ConfigureAwait(false);

    public async Task<NutrientExtractionCache> SaveExtractionCacheAsync(NutrientExtractionCache entry, CancellationToken ct = default)
    {
        drContext.NutrientExtractionCache.Add(entry);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return entry;
    }

    /// <summary>Deletes all extraction cache entries older than the given cutoff; returns rows deleted.</summary>
    public async Task<int> DeleteExpiredCacheAsync(DateTime cutoff, CancellationToken ct = default) =>
        await drContext.NutrientExtractionCache
            .Where(c => c.CreatedAt < cutoff)
            .ExecuteDeleteAsync(ct)
            .ConfigureAwait(false);
}
