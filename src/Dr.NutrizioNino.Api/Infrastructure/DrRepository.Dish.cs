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

        if (dish is null)
        {
            return null;
        }

        return new DishDetailDto(
            dish.Id,
            dish.Name,
            dish.WeightGrams,
            dish.DishIngredients
                .Select(di => new DishDetailIngredientDto(di.FoodId, di.Food.Name, di.QuantityGrams))
                .ToList(),
            dish.DishNutrients
                .OrderBy(dn => dn.Nutrient.PositionOrder)
                .Select(dn => new DishDetailNutrientDto(dn.NutrientId, dn.Nutrient.Name, dn.Nutrient.PositionOrder, dn.UnitOfMeasureId, dn.Quantity))
                .ToList()
        );
    }

    /// <summary>Carica il piatto con i soli ingredienti, usato per il ricalcolo.</summary>
    internal async Task<Dish?> GetDishWithIngredientsAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Dishes
            .Include(d => d.DishIngredients)
            .FirstOrDefaultAsync(d => d.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<Guid?> GetDishOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Dishes
            .AsNoTracking()
            .Where(d => d.Id == id)
            .Select(d => d.OwnerId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

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

        if (record is null)
        {
            return;
        }

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.DishNutrients.RemoveRange(record.DishNutrients);
        drContext.DishIngredients.RemoveRange(record.DishIngredients);
        drContext.Dishes.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
    }

    public async Task<IEnumerable<DishDashboardInfo>> GetDishesDashboardAsync(CancellationToken ct = default) =>
        await drContext.DishesDashboard
            .AsNoTracking()
            .ToListAsync(ct)
            .ConfigureAwait(false);

    /// <summary>Aggiorna peso e nutrienti del piatto in un'unica transazione; azzera il flag stale.</summary>
    public async Task<bool> UpdateDishNutrientsAsync(Guid dishId, decimal newWeightGrams, IList<DishNutrient> newNutrients, CancellationToken ct = default)
    {
        var record = await drContext.Dishes
            .Include(d => d.DishNutrients)
            .FirstOrDefaultAsync(d => d.Id == dishId, ct)
            .ConfigureAwait(false);

        if (record is null)
        {
            return false;
        }

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        record.WeightGrams = newWeightGrams;
        record.IsNutritionStale = false;
        record.NutrientsCalculatedAt = DateTime.UtcNow;

        drContext.DishNutrients.RemoveRange(record.DishNutrients);
        drContext.DishNutrients.AddRange(newNutrients);

        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return true;
    }

    /// <summary>Riscala proporzionalmente calorie e nutrienti al nuovo peso; non tocca gli ingredienti.</summary>
    public async Task<bool> RescaleDishAsync(Guid dishId, decimal newWeightGrams, CancellationToken ct = default)
    {
        var record = await drContext.Dishes
            .Include(d => d.DishNutrients)
            .FirstOrDefaultAsync(d => d.Id == dishId, ct)
            .ConfigureAwait(false);

        if (record is null)
        {
            return false;
        }

        if (record.WeightGrams == 0)
        {
            return false;
        }

        var ratio = newWeightGrams / record.WeightGrams;

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        record.WeightGrams = newWeightGrams;

        foreach (var dn in record.DishNutrients)
        {
            dn.Quantity = Math.Round(dn.Quantity * ratio, 2);
        }

        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return true;
    }

    /// <summary>Marca stale tutti i piatti che contengono il cibo specificato tra gli ingredienti.</summary>
    public async Task MarkDishesStaleByFoodIdAsync(Guid foodId, CancellationToken ct = default) =>
        await drContext.Dishes
            .Where(d => d.DishIngredients.Any(di => di.FoodId == foodId))
            .ExecuteUpdateAsync(s => s.SetProperty(d => d.IsNutritionStale, true), ct)
            .ConfigureAwait(false);

    /// <summary>Restituisce gli id dei piatti con flag stale attivo.</summary>
    public async Task<IEnumerable<Guid>> GetStaleDishIdsAsync(CancellationToken ct = default) =>
        await drContext.Dishes
            .AsNoTracking()
            .Where(d => d.IsNutritionStale)
            .Select(d => d.Id)
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
