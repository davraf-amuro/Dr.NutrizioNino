using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<Category> CreateCategoryAsync(Category category, CancellationToken ct = default)
    {
        drContext.Categories.Add(category);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.Categories.FindAsync(new object?[] { id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        drContext.Categories.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<TResult?> GetCategoryAsync<TResult>(
        Guid id,
        Expression<Func<Category, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task<IList<TResult>> GetCategoriesAsync<TResult>(
        Expression<Func<Category, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(selector)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<bool> UpdateCategoryAsync(Category category, CancellationToken ct = default)
    {
        var record = await drContext.Categories.FindAsync(new object?[] { category.Id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        record.Name = category.Name;
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> IsCategoryInUseAsync(Guid id, CancellationToken ct = default) =>
        await drContext.FoodCategories.AsNoTracking().AnyAsync(fc => fc.CategoryId == id, ct).ConfigureAwait(false);

    public async Task<bool> IsCategoryNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drContext.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || c.Id != excludeId.Value), ct)
            .ConfigureAwait(false);
}
