using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<IEnumerable<Food>> GetFoodsByIdsAsync(IList<Guid> foodIds, CancellationToken ct = default) =>
        await drContext.Foods
            .AsNoTracking()
            .Where(f => foodIds.Contains(f.Id))
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<IEnumerable<FoodNutrient>> GetNutrientsForFoodsAsync(IList<Guid> foodIds, CancellationToken ct = default) =>
        await drContext.FoodsNutrients
            .AsNoTracking()
            .Where(fn => foodIds.Contains(fn.FoodId))
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<Guid> CreateDishAsync(Food dish, IList<DishIngredient> ingredients, CancellationToken ct = default)
    {
        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.Foods.Add(dish);
        drContext.DishIngredients.AddRange(ingredients);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return dish.Id;
    }

    public async Task DeleteDishAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.Foods
            .Include(f => f.FoodsNutrients)
            .Include(f => f.DishIngredients)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            .ConfigureAwait(false);

        if (record is null) return;

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.DishIngredients.RemoveRange(record.DishIngredients);
        drContext.FoodsNutrients.RemoveRange(record.FoodsNutrients);
        drContext.Foods.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
    }

    public async Task<IEnumerable<FoodDashboardInfo>> GetDishesDashboardAsync(CancellationToken ct = default) =>
        await drContext.Foods
            .AsNoTracking()
            .Where(f => f.IsDish)
            .Select(f => new FoodDashboardInfo
            {
                Id = f.Id,
                Name = f.Name,
                Quantity = f.Quantity,
                Calorie = f.Calorie,
                BrandDescription = null,
                UnitOfMeasureDescription = f.UnitOfMeasure.Name,
                Abbreviation = f.UnitOfMeasure.Abbreviation
            })
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
