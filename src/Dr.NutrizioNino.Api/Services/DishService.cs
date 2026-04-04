using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class DishService(DrRepository drRepository)
{
    public async Task<DishDetailDto?> GetDishDetailAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetDishByIdAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsDishNameTakenAsync(string name, CancellationToken ct = default) =>
        await drRepository.IsDishNameTakenAsync(name, ct).ConfigureAwait(false);

    public async Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetDishOwnerIdAsync(id, ct).ConfigureAwait(false);

    public async Task<(DishDetailDto? Dto, string? Error)> CreateDishAsync(CreateDishDto dto, Guid? ownerId = null, CancellationToken ct = default)
    {
        if (dto.Ingredients.Count == 0)
        {
            return (null, "Il piatto deve avere almeno un ingrediente.");
        }

        var foodIds = dto.Ingredients.Select(i => i.FoodId).Distinct().ToList();
        var foods = (await drRepository.GetFoodsByIdsAsync(foodIds, ct)).ToList();

        var missingCount = foodIds.Except(foods.Select(f => f.Id)).Count();
        if (missingCount > 0)
        {
            return (null, $"{missingCount} alimento/i non trovato/i.");
        }

        var allNutrients = (await drRepository.GetNutrientsForFoodsAsync(foodIds, ct)).ToList();
        var totalWeight = dto.Ingredients.Sum(i => i.QuantityGrams);

        var contributions = new Dictionary<Guid, (Guid UomId, decimal Total)>();
        foreach (var ingredient in dto.Ingredients)
        {
            foreach (var fn in allNutrients.Where(n => n.FoodId == ingredient.FoodId))
            {
                var added = fn.Quantity * (ingredient.QuantityGrams / 100m);
                if (contributions.TryGetValue(fn.NutrientId, out var existing))
                {
                    contributions[fn.NutrientId] = (existing.UomId, existing.Total + added);
                }
                else
                {
                    contributions[fn.NutrientId] = (fn.UnitOfMeasureId, added);
                }
            }
        }

        var dishId = Guid.NewGuid();
        var dish = new Dish
        {
            Id = dishId,
            Name = dto.Name,
            WeightGrams = totalWeight,
            UnitOfMeasureId = Constants.GetDefaultUnitOfMeasure(),
            IsNutritionStale = false,
            NutrientsCalculatedAt = DateTime.UtcNow,
            OwnerId = ownerId
        };

        foreach (var kv in contributions)
        {
            dish.DishNutrients.Add(new DishNutrient
            {
                DishId = dishId,
                NutrientId = kv.Key,
                UnitOfMeasureId = kv.Value.UomId,
                Quantity = Math.Round(kv.Value.Total, 2)
            });
        }

        var ingredients = dto.Ingredients.Select(i => new DishIngredient
        {
            DishId = dishId,
            FoodId = i.FoodId,
            QuantityGrams = i.QuantityGrams
        }).ToList();

        await drRepository.CreateDishAsync(dish, ingredients, ct).ConfigureAwait(false);
        var detail = await drRepository.GetDishByIdAsync(dishId, ct).ConfigureAwait(false);
        return (detail, null);
    }

    public async Task<IList<DishDashboardInfo>> GetDishesDashboardAsync(CancellationToken ct = default) =>
        (await drRepository.GetDishesDashboardAsync(ct).ConfigureAwait(false)).ToList();

    public async Task DeleteDishAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.DeleteDishAsync(id, ct).ConfigureAwait(false);

    /// <summary>
    /// Ricalcola calorie e nutrienti del piatto a partire dagli ingredienti correnti.
    /// Azzera IsNutritionStale e aggiorna NutrientsCalculatedAt.
    /// </summary>
    public async Task<(bool Found, string? Error)> RecalculateDishAsync(Guid id, CancellationToken ct = default)
    {
        var dish = await drRepository.GetDishWithIngredientsAsync(id, ct).ConfigureAwait(false);
        if (dish is null)
        {
            return (false, null);
        }

        if (dish.DishIngredients.Count == 0)
        {
            return (true, "Il piatto non ha ingredienti: nessun ricalcolo effettuato.");
        }

        var foodIds = dish.DishIngredients.Select(di => di.FoodId).Distinct().ToList();
        var foods = (await drRepository.GetFoodsByIdsAsync(foodIds, ct)).ToList();
        var allNutrients = (await drRepository.GetNutrientsForFoodsAsync(foodIds, ct)).ToList();

        var totalWeight = dish.DishIngredients.Sum(di => di.QuantityGrams);

        var contributions = new Dictionary<Guid, (Guid UomId, decimal Total)>();
        foreach (var ingredient in dish.DishIngredients)
        {
            foreach (var fn in allNutrients.Where(n => n.FoodId == ingredient.FoodId))
            {
                var added = fn.Quantity * (ingredient.QuantityGrams / 100m);
                if (contributions.TryGetValue(fn.NutrientId, out var existing))
                {
                    contributions[fn.NutrientId] = (existing.UomId, existing.Total + added);
                }
                else
                {
                    contributions[fn.NutrientId] = (fn.UnitOfMeasureId, added);
                }
            }
        }

        var newNutrients = contributions.Select(kv => new DishNutrient
        {
            DishId = id,
            NutrientId = kv.Key,
            UnitOfMeasureId = kv.Value.UomId,
            Quantity = Math.Round(kv.Value.Total, 2)
        }).ToList();

        await drRepository.UpdateDishNutrientsAsync(id, totalWeight, newNutrients, ct).ConfigureAwait(false);
        return (true, null);
    }

    /// <summary>
    /// Aggiorna il peso del piatto. Se <paramref name="recalculate"/> è true ricalcola dagli ingredienti;
    /// altrimenti applica un rescaling proporzionale O(1) senza accedere ai Foods.
    /// </summary>
    public async Task<(bool Found, string? Error)> UpdateWeightAsync(Guid id, decimal newWeightGrams, bool recalculate, CancellationToken ct = default)
    {
        if (recalculate)
        {
            return await RecalculateDishAsync(id, ct).ConfigureAwait(false);
        }

        var found = await drRepository.RescaleDishAsync(id, newWeightGrams, ct).ConfigureAwait(false);
        return (found, null);
    }

    /// <summary>
    /// Ricalcola tutti i piatti con IsNutritionStale = true.
    /// Restituisce il numero di piatti aggiornati.
    /// </summary>
    public async Task<int> RecalculateAllStaleDishesAsync(CancellationToken ct = default)
    {
        var staleIds = (await drRepository.GetStaleDishIdsAsync(ct).ConfigureAwait(false)).ToList();
        var count = 0;
        foreach (var staleId in staleIds)
        {
            var (found, _) = await RecalculateDishAsync(staleId, ct).ConfigureAwait(false);
            if (found)
            {
                count++;
            }
        }
        return count;
    }
}
