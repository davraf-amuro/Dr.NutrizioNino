using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<Brand> CreateBrandAsync(Brand brand)
    {
        drContext.Brands.Add(brand);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return brand;
    }

    public async Task<bool> DeleteBrandAsync(Guid id)
    {
        var record = await drContext.Brands.FindAsync(id).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        drContext.Brands.Remove(record);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<Brand?> GetBrandAsync(Guid id)
    {
        return await drContext.Brands
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<Brand>> GetBrandsAsync() =>
        await drContext.Brands.AsNoTracking().ToListAsync().ConfigureAwait(false);

    public async Task<bool> UpdateBrandAsync(Brand brand)
    {
        var record = await drContext.Brands.FindAsync(brand.Id).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        record.Name = brand.Name;
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<bool> IsBrandInUseAsync(Guid id) =>
        await drContext.Foods.AnyAsync(f => f.BrandId == id).ConfigureAwait(false);
}
