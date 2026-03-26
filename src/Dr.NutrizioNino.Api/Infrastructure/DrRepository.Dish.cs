using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;
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

    public async Task<DishDetailDto?> GetDishByIdAsync(Guid id, CancellationToken ct = default)
    {
        var dish = await drContext.Dishes
            .AsNoTracking()
            .Include(d => d.DishNutrients).ThenInclude(dn => dn.Nutrient)
            .Include(d => d.DishIngredients).ThenInclude(di => di.Food)
            .FirstOrDefaultAsync(d => d.Id == id, ct)
            .ConfigureAwait(false);

        if (dish is null) return null;

        return new DishDetailDto(
            dish.Id,
            dish.Name,
            dish.Calorie,
            dish.DishIngredients
                .Select(di => new DishDetailIngredientDto(di.FoodId, di.Food.Name, di.QuantityGrams))
                .ToList(),
            dish.DishNutrients
                .OrderBy(dn => dn.Nutrient.PositionOrder)
                .Select(dn => new DishDetailNutrientDto(dn.NutrientId, dn.Nutrient.Name, dn.Nutrient.PositionOrder, dn.UnitOfMeasureId, dn.Quantity))
                .ToList()
        );
    }

    public async Task<bool> IsDishNameTakenAsync(string name, CancellationToken ct = default) =>
        await drContext.Dishes
            .AnyAsync(d => d.Name.ToLower() == name.ToLower(), ct)
            .ConfigureAwait(false);

    public async Task<Guid> CreateDishAsync(Dish dish, IList<DishIngredient> ingredients, CancellationToken ct = default)
    {
        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.Dishes.Add(dish);
        drContext.DishIngredients.AddRange(ingredients);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return dish.Id;
    }

    public async Task DeleteDishAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.Dishes
            .Include(d => d.DishNutrients)
            .Include(d => d.DishIngredients)
            .FirstOrDefaultAsync(d => d.Id == id, ct)
            .ConfigureAwait(false);

        if (record is null) return;

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.DishNutrients.RemoveRange(record.DishNutrients);
        drContext.DishIngredients.RemoveRange(record.DishIngredients);
        drContext.Dishes.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
    }

    public async Task<IEnumerable<FoodDashboardInfo>> GetDishesDashboardAsync(CancellationToken ct = default) =>
        await drContext.Dishes
            .AsNoTracking()
            .Select(d => new FoodDashboardInfo
            {
                Id = d.Id,
                Name = d.Name,
                Quantity = d.Quantity,
                Calorie = d.Calorie,
                BrandDescription = null,
                UnitOfMeasureDescription = d.UnitOfMeasure.Name,
                Abbreviation = d.UnitOfMeasure.Abbreviation,
                IsDish = true
            })
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
