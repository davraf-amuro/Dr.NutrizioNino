using System.Linq.Expressions;
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

    public async Task<TResult?> GetBrandAsync<TResult>(
        Guid id,
        Expression<Func<Brand, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.Brands
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task<IList<TResult>> GetBrandsAsync<TResult>(
        Expression<Func<Brand, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.Brands
            .AsNoTracking()
            .Select(selector)
            .ToListAsync(ct)
            .ConfigureAwait(false);

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

    public async Task<Guid?> GetBrandOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Brands
            .AsNoTracking()
            .Where(b => b.Id == id)
            .Select(b => b.OwnerId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task<bool> IsBrandInUseAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Foods.AsNoTracking().AnyAsync(f => f.BrandId == id, ct).ConfigureAwait(false);

    public async Task<bool> IsBrandNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drContext.Brands
            .AsNoTracking()
            .AnyAsync(b => b.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || b.Id != excludeId.Value), ct)
            .ConfigureAwait(false);
}
