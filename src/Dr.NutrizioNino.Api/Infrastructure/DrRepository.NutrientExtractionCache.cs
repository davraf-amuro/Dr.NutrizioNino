using Dr.NutrizioNino.Api.Infrastructure.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    private const int SqlUniqueViolationErrorNumber = 2627;

    /// <summary>Returns cached extraction for the given image hash + provider combination.</summary>
    public async Task<NutrientExtractionCache?> GetCacheByHashAsync(string imageHash, string providerKey, CancellationToken ct = default) =>
        await drContext.NutrientExtractionCache
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ImageHash == imageHash && c.ProviderKey == providerKey, ct)
            .ConfigureAwait(false);

    /// <summary>Inserts extraction cache entry; on concurrent insert race (same hash+provider) returns the row already saved by the other request instead of throwing.</summary>
    public async Task<NutrientExtractionCache> SaveExtractionCacheAsync(NutrientExtractionCache entry, CancellationToken ct = default)
    {
        drContext.NutrientExtractionCache.Add(entry);
        try
        {
            await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return entry;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: SqlUniqueViolationErrorNumber })
        {
            drContext.Entry(entry).State = EntityState.Detached;
            var existing = await GetCacheByHashAsync(entry.ImageHash, entry.ProviderKey, ct).ConfigureAwait(false);
            if (existing is not null)
            {
                return existing;
            }

            throw;
        }
    }

    /// <summary>Deletes all extraction cache entries older than the given cutoff; returns rows deleted.</summary>
    public async Task<int> DeleteExpiredCacheAsync(DateTime cutoff, CancellationToken ct = default) =>
        await drContext.NutrientExtractionCache
            .Where(c => c.CreatedAt < cutoff)
            .ExecuteDeleteAsync(ct)
            .ConfigureAwait(false);
}
