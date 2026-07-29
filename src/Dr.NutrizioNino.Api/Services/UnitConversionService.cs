using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Services;

public class UnitConversionService(IDbContextFactory<DrNutrizioNinoContext> dbFactory)
{
    private Dictionary<(string, string), decimal>? _cache;

    public async Task<decimal> ConvertAsync(decimal value, string from, string to)
    {
        if (from == to)
        {
            return value;
        }

        var map = await GetMapAsync().ConfigureAwait(false);
        if (map.TryGetValue((from, to), out var factor))
        {
            return value * factor;
        }

        throw new NotSupportedException($"Conversione {from}→{to} non supportata");
    }

    public Task ReloadAsync()
    {
        _cache = null;
        return Task.CompletedTask;
    }

    private async Task<Dictionary<(string, string), decimal>> GetMapAsync()
    {
        if (_cache is not null)
        {
            return _cache;
        }

        await using var db = await dbFactory.CreateDbContextAsync().ConfigureAwait(false);
        _cache = await db.UnitConversions!
            .AsNoTracking()
            .ToDictionaryAsync(r => (r.FromUnit, r.ToUnit), r => r.Factor)
            .ConfigureAwait(false);
        return _cache;
    }
}
