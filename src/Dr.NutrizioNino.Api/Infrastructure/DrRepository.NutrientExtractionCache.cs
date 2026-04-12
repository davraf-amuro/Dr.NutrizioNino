using Dr.NutrizioNino.Api.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<NutrientExtractionCache?> GetCacheByHashAsync(string imageHash, CancellationToken ct = default) =>
        await drContext.NutrientExtractionCache
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ImageHash == imageHash, ct)
            .ConfigureAwait(false);

    public async Task<NutrientExtractionCache> SaveExtractionCacheAsync(NutrientExtractionCache entry, CancellationToken ct = default)
    {
        drContext.NutrientExtractionCache.Add(entry);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return entry;
    }
}
