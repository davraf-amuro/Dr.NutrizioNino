using Dr.NutrizioNino.Api.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    /// <summary>Returns the alias entry for the given AI-extracted name, or null if not found.</summary>
    public async Task<NutrientAlias?> GetAliasByAiNameAsync(string aiName, CancellationToken ct = default) =>
        await drContext.NutrientAliases
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AiName == aiName, ct)
            .ConfigureAwait(false);

    /// <summary>Returns all aliases as a dictionary keyed by AiName for O(1) lookup.</summary>
    public async Task<Dictionary<string, NutrientAlias>> GetAllAliasesAsync(CancellationToken ct = default) =>
        await drContext.NutrientAliases
            .AsNoTracking()
            .ToDictionaryAsync(a => a.AiName, StringComparer.OrdinalIgnoreCase, ct)
            .ConfigureAwait(false);

    /// <summary>Persists a new alias mapping (AI name → canonical nutrient).</summary>
    public async Task<NutrientAlias> SaveAliasAsync(NutrientAlias alias, CancellationToken ct = default)
    {
        drContext.NutrientAliases.Add(alias);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return alias;
    }
}
