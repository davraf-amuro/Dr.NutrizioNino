using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<Nutrient> CreateNutrientAsync(Nutrient nutrient, CancellationToken ct = default)
    {
        drContext.Nutrients.Add(nutrient);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return nutrient;
    }

    public async Task<bool> DeleteNutrientAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.Nutrients.FindAsync(new object?[] { id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        drContext.Nutrients.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> NutrientNameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drContext.Nutrients
            .AsNoTracking()
            .AnyAsync(n => n.Name == name && (excludeId == null || n.Id != excludeId), ct)
            .ConfigureAwait(false);

    public async Task<bool> IsNutrientInUseAsync(Guid id, CancellationToken ct = default) =>
        await drContext.FoodsNutrients
            .AsNoTracking()
            .AnyAsync(fn => fn.NutrientId == id, ct)
            .ConfigureAwait(false);

    public async Task<TResult?> GetNutrientAsync<TResult>(
        Guid id,
        Expression<Func<Nutrient, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.Nutrients
            .AsNoTracking()
            .Where(n => n.Id == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task<IList<TResult>> GetNutrientsAsync<TResult>(
        Expression<Func<Nutrient, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.Nutrients
            .AsNoTracking()
            .Select(selector)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<bool> UpdateNutrientAsync(Nutrient nutrient, CancellationToken ct = default)
    {
        var record = await drContext.Nutrients.FindAsync(new object?[] { nutrient.Id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        record.Name = nutrient.Name;
        record.PositionOrder = nutrient.PositionOrder;
        record.DefaultQuantity = nutrient.DefaultQuantity;
        record.DefaultUnitOfMeasureId = nutrient.DefaultUnitOfMeasureId;

        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    /// <summary>
    /// Restituisce tutti i nutrienti. Se riceve il guid di un cibo restituisce i nutrienti di quel cibo più quelli mancanti.
    /// </summary>
    public async Task<IEnumerable<NutrientsGetForFoodCreatingInfo>> GetAllNutrientsForFood(Guid? id, CancellationToken ct = default)
    {
        return await drContext.NutrientsGetForFoodCreatingInfoes
            .FromSql($"EXECUTE dbo.Full_Nutrients_For_Food {id}")
            .AsNoTracking()
            .ToListAsync(ct)
            .ConfigureAwait(false);
    }
}
