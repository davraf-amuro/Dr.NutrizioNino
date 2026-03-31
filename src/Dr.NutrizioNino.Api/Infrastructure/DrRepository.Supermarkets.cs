using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<Supermarket> CreateSupermarketAsync(Supermarket supermarket, CancellationToken ct = default)
    {
        drContext.Supermarkets.Add(supermarket);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return supermarket;
    }

    public async Task<bool> DeleteSupermarketAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.Supermarkets.FindAsync(new object?[] { id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        drContext.Supermarkets.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<Supermarket?> GetSupermarketAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Supermarkets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Supermarket>> GetSupermarketsAsync(CancellationToken ct = default) =>
        await drContext.Supermarkets.AsNoTracking().OrderBy(s => s.Name).ToListAsync(ct).ConfigureAwait(false);

    public async Task<bool> UpdateSupermarketAsync(Supermarket supermarket, CancellationToken ct = default)
    {
        var record = await drContext.Supermarkets.FindAsync(new object?[] { supermarket.Id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        record.Name = supermarket.Name;
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> IsSupermarketInUseAsync(Guid id, CancellationToken ct = default) =>
        await drContext.FoodSupermarkets.AnyAsync(fs => fs.SupermarketId == id, ct).ConfigureAwait(false);

    public async Task<bool> IsSupermarketNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drContext.Supermarkets
            .AnyAsync(s => s.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || s.Id != excludeId.Value), ct)
            .ConfigureAwait(false);
}
