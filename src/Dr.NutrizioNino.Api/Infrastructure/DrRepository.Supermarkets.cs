using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<Supermarket> CreateSupermarketAsync(Supermarket supermarket)
    {
        drContext.Supermarkets.Add(supermarket);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return supermarket;
    }

    public async Task<bool> DeleteSupermarketAsync(Guid id)
    {
        var record = await drContext.Supermarkets.FindAsync(id).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        drContext.Supermarkets.Remove(record);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<Supermarket?> GetSupermarketAsync(Guid id) =>
        await drContext.Supermarkets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Supermarket>> GetSupermarketsAsync() =>
        await drContext.Supermarkets.AsNoTracking().OrderBy(s => s.Name).ToListAsync().ConfigureAwait(false);

    public async Task<bool> UpdateSupermarketAsync(Supermarket supermarket)
    {
        var record = await drContext.Supermarkets.FindAsync(supermarket.Id).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        record.Name = supermarket.Name;
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<bool> IsSupermarketInUseAsync(Guid id) =>
        await drContext.FoodSupermarkets.AnyAsync(fs => fs.SupermarketId == id).ConfigureAwait(false);

    public async Task<bool> IsSupermarketNameTakenAsync(string name, Guid? excludeId = null) =>
        await drContext.Supermarkets
            .AnyAsync(s => s.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || s.Id != excludeId.Value))
            .ConfigureAwait(false);
}
