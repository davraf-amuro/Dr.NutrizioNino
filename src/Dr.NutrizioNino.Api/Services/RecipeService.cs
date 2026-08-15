using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class RecipeService(DrRepository drRepository)
{
    public async Task<RecipeDetailDto?> GetRecipeDetailAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetRecipeByIdAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsRecipeNameTakenAsync(string name, CancellationToken ct = default) =>
        await drRepository.IsRecipeNameTakenAsync(name, ct).ConfigureAwait(false);

    public async Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetRecipeOwnerIdAsync(id, ct).ConfigureAwait(false);

    public async Task<(RecipeDetailDto? Dto, string? Error)> CreateRecipeAsync(CreateRecipeDto dto, Guid? ownerId = null, CancellationToken ct = default)
    {
        if (dto.Ingredients.Count == 0)
        {
            return (null, "La ricetta deve avere almeno un ingrediente.");
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

        var recipeId = Guid.NewGuid();
        var recipe = new Recipe
        {
            Id = recipeId,
            Name = dto.Name,
            WeightGrams = totalWeight,
            UnitOfMeasureId = Constants.GetDefaultUnitOfMeasure(),
            IsNutritionStale = false,
            NutrientsCalculatedAt = DateTime.UtcNow,
            OwnerId = ownerId
        };

        foreach (var kv in contributions)
        {
            recipe.RecipeNutrients.Add(new RecipeNutrient
            {
                RecipeId = recipeId,
                NutrientId = kv.Key,
                UnitOfMeasureId = kv.Value.UomId,
                Quantity = Math.Round(kv.Value.Total, 2)
            });
        }

        var ingredients = dto.Ingredients.Select(i => new RecipeIngredient
        {
            RecipeId = recipeId,
            FoodId = i.FoodId,
            QuantityGrams = i.QuantityGrams
        }).ToList();

        await drRepository.CreateRecipeAsync(recipe, ingredients, ct).ConfigureAwait(false);
        var detail = await drRepository.GetRecipeByIdAsync(recipeId, ct).ConfigureAwait(false);
        return (detail, null);
    }

    public async Task<IList<RecipeDashboardInfo>> GetRecipesDashboardAsync(CancellationToken ct = default) =>
        (await drRepository.GetRecipesDashboardAsync(ct).ConfigureAwait(false)).ToList();

    public async Task DeleteRecipeAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.DeleteRecipeAsync(id, ct).ConfigureAwait(false);

    /// <summary>
    /// Ricalcola calorie e nutrienti della ricetta a partire dagli ingredienti correnti.
    /// Azzera IsNutritionStale e aggiorna NutrientsCalculatedAt.
    /// </summary>
    public async Task<(bool Found, string? Error)> RecalculateRecipeAsync(Guid id, CancellationToken ct = default)
    {
        var recipe = await drRepository.GetRecipeWithIngredientsAsync(id, ct).ConfigureAwait(false);
        if (recipe is null)
        {
            return (false, null);
        }

        if (recipe.RecipeIngredients.Count == 0)
        {
            return (true, "La ricetta non ha ingredienti: nessun ricalcolo effettuato.");
        }

        var foodIds = recipe.RecipeIngredients.Select(ri => ri.FoodId).Distinct().ToList();
        var foods = (await drRepository.GetFoodsByIdsAsync(foodIds, ct)).ToList();
        var allNutrients = (await drRepository.GetNutrientsForFoodsAsync(foodIds, ct)).ToList();

        var totalWeight = recipe.RecipeIngredients.Sum(ri => ri.QuantityGrams);

        var contributions = new Dictionary<Guid, (Guid UomId, decimal Total)>();
        foreach (var ingredient in recipe.RecipeIngredients)
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

        var newNutrients = contributions.Select(kv => new RecipeNutrient
        {
            RecipeId = id,
            NutrientId = kv.Key,
            UnitOfMeasureId = kv.Value.UomId,
            Quantity = Math.Round(kv.Value.Total, 2)
        }).ToList();

        await drRepository.UpdateRecipeNutrientsAsync(id, totalWeight, newNutrients, ct).ConfigureAwait(false);
        return (true, null);
    }

    /// <summary>
    /// Aggiorna il peso della ricetta. Se <paramref name="recalculate"/> è true ricalcola dagli ingredienti;
    /// altrimenti applica un rescaling proporzionale O(1) senza accedere ai Foods.
    /// </summary>
    public async Task<(bool Found, string? Error)> UpdateWeightAsync(Guid id, decimal newWeightGrams, bool recalculate, CancellationToken ct = default)
    {
        if (recalculate)
        {
            return await RecalculateRecipeAsync(id, ct).ConfigureAwait(false);
        }

        var found = await drRepository.RescaleRecipeAsync(id, newWeightGrams, ct).ConfigureAwait(false);
        return (found, null);
    }

    /// <summary>
    /// Ricalcola tutte le ricette con IsNutritionStale = true.
    /// Restituisce il numero di ricette aggiornate.
    /// </summary>
    public async Task<int> RecalculateAllStaleRecipesAsync(CancellationToken ct = default)
    {
        var staleIds = (await drRepository.GetStaleRecipeIdsAsync(ct).ConfigureAwait(false)).ToList();
        var count = 0;
        foreach (var staleId in staleIds)
        {
            var (found, _) = await RecalculateRecipeAsync(staleId, ct).ConfigureAwait(false);
            if (found)
            {
                count++;
            }
        }
        return count;
    }
}
