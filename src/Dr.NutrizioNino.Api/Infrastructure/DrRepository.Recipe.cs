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

    /// <summary>
    /// Carica in una sola query le ricette richieste con i soli nutrienti aggregati, per il confronto.
    /// Sola lettura: non tocca gli ingredienti e non scrive nulla.
    /// </summary>
    public async Task<IEnumerable<Recipe>> GetRecipesForComparisonAsync(IReadOnlyList<Guid> ids, CancellationToken ct = default) =>
        // Un solo round-trip con IN (@ids): niente N chiamate a GetRecipeByIdAsync, niente Include sugli ingredienti.
        await drContext.Recipes
            .AsNoTracking()
            .Where(r => ids.Contains(r.Id))
            .Include(r => r.RecipeNutrients).ThenInclude(rn => rn.Nutrient)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<RecipeDetailDto?> GetRecipeByIdAsync(Guid id, CancellationToken ct = default)
    {
        var recipe = await drContext.Recipes
            .AsNoTracking()
            .Include(r => r.RecipeNutrients).ThenInclude(rn => rn.Nutrient)
            .Include(r => r.RecipeIngredients).ThenInclude(ri => ri.Food)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            .ConfigureAwait(false);

        if (recipe is null)
        {
            return null;
        }

        return new RecipeDetailDto(
            recipe.Id,
            recipe.Name,
            recipe.WeightGrams,
            recipe.RecipeIngredients
                .Select(ri => new RecipeDetailIngredientDto(ri.FoodId, ri.Food.Name, ri.QuantityGrams))
                .ToList(),
            recipe.RecipeNutrients
                .OrderBy(rn => rn.Nutrient.PositionOrder)
                .Select(rn => new RecipeDetailNutrientDto(rn.NutrientId, rn.Nutrient.Name, rn.Nutrient.PositionOrder, rn.UnitOfMeasureId, rn.Quantity))
                .ToList()
        );
    }

    /// <summary>Carica la ricetta con i soli ingredienti, usata per il ricalcolo.</summary>
    internal async Task<Recipe?> GetRecipeWithIngredientsAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Recipes
            .Include(r => r.RecipeIngredients)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<Guid?> GetRecipeOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Recipes
            .AsNoTracking()
            .Where(r => r.Id == id)
            .Select(r => r.OwnerId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task<bool> IsRecipeNameTakenAsync(string name, CancellationToken ct = default) =>
        await drContext.Recipes
            .AnyAsync(r => r.Name.ToLower() == name.ToLower(), ct)
            .ConfigureAwait(false);

    public async Task<Guid> CreateRecipeAsync(Recipe recipe, IList<RecipeIngredient> ingredients, CancellationToken ct = default)
    {
        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.Recipes.Add(recipe);
        drContext.RecipeIngredients.AddRange(ingredients);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return recipe.Id;
    }

    public async Task DeleteRecipeAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.Recipes
            .Include(r => r.RecipeNutrients)
            .Include(r => r.RecipeIngredients)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            .ConfigureAwait(false);

        if (record is null)
        {
            return;
        }

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.RecipeNutrients.RemoveRange(record.RecipeNutrients);
        drContext.RecipeIngredients.RemoveRange(record.RecipeIngredients);
        drContext.Recipes.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
    }

    public async Task<IEnumerable<RecipeDashboardInfo>> GetRecipesDashboardAsync(CancellationToken ct = default) =>
        await drContext.RecipesDashboard
            .AsNoTracking()
            .ToListAsync(ct)
            .ConfigureAwait(false);

    /// <summary>Aggiorna peso e nutrienti della ricetta in un'unica transazione; azzera il flag stale.</summary>
    public async Task<bool> UpdateRecipeNutrientsAsync(Guid recipeId, decimal newWeightGrams, IList<RecipeNutrient> newNutrients, CancellationToken ct = default)
    {
        var record = await drContext.Recipes
            .Include(r => r.RecipeNutrients)
            .FirstOrDefaultAsync(r => r.Id == recipeId, ct)
            .ConfigureAwait(false);

        if (record is null)
        {
            return false;
        }

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        record.WeightGrams = newWeightGrams;
        record.IsNutritionStale = false;
        record.NutrientsCalculatedAt = DateTime.UtcNow;

        drContext.RecipeNutrients.RemoveRange(record.RecipeNutrients);
        drContext.RecipeNutrients.AddRange(newNutrients);

        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return true;
    }

    /// <summary>Riscala proporzionalmente calorie e nutrienti al nuovo peso; non tocca gli ingredienti.</summary>
    public async Task<bool> RescaleRecipeAsync(Guid recipeId, decimal newWeightGrams, CancellationToken ct = default)
    {
        var record = await drContext.Recipes
            .Include(r => r.RecipeNutrients)
            .FirstOrDefaultAsync(r => r.Id == recipeId, ct)
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

        foreach (var rn in record.RecipeNutrients)
        {
            rn.Quantity = Math.Round(rn.Quantity * ratio, 2);
        }

        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return true;
    }

    /// <summary>Marca stale tutte le ricette che contengono il cibo specificato tra gli ingredienti.</summary>
    public async Task MarkRecipesStaleByFoodIdAsync(Guid foodId, CancellationToken ct = default) =>
        await drContext.Recipes
            .Where(r => r.RecipeIngredients.Any(ri => ri.FoodId == foodId))
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsNutritionStale, true), ct)
            .ConfigureAwait(false);

    /// <summary>Restituisce gli id delle ricette con flag stale attivo.</summary>
    public async Task<IEnumerable<Guid>> GetStaleRecipeIdsAsync(CancellationToken ct = default) =>
        await drContext.Recipes
            .AsNoTracking()
            .Where(r => r.IsNutritionStale)
            .Select(r => r.Id)
            .ToListAsync(ct)
            .ConfigureAwait(false);
}
