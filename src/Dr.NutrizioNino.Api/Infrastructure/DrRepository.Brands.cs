using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<Brand> CreateBrandAsync(Brand brand, CancellationToken ct = default)
    {
        drContext.Brands.Add(brand);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return brand;
    }

    public async Task<bool> DeleteBrandAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.Brands.FindAsync(new object?[] { id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        drContext.Brands.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<Brand?> GetBrandAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Brands
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<IEnumerable<Brand>> GetBrandsAsync(CancellationToken ct = default) =>
        await drContext.Brands.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);

    public async Task<bool> UpdateBrandAsync(Brand brand, CancellationToken ct = default)
    {
        var record = await drContext.Brands.FindAsync(new object?[] { brand.Id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        record.Name = brand.Name;
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> IsBrandInUseAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Foods.AnyAsync(f => f.BrandId == id, ct).ConfigureAwait(false);

    public async Task<bool> IsBrandNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drContext.Brands
            .AnyAsync(b => b.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || b.Id != excludeId.Value), ct)
            .ConfigureAwait(false);
}
