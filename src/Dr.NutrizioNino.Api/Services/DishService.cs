using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class DishService(DrRepository drRepository)
{
    public async Task<bool> IsDishNameTakenAsync(string name) =>
        await drRepository.IsFoodNameTakenAsync(name).ConfigureAwait(false);

    public async Task<(Guid? Id, string? Error)> CreateDishAsync(CreateDishDto dto, CancellationToken ct = default)
    {
        if (dto.Ingredients.Count == 0)
            return (null, "Il piatto deve avere almeno un ingrediente.");

        var foodIds = dto.Ingredients.Select(i => i.FoodId).Distinct().ToList();
        var foods = (await drRepository.GetFoodsByIdsAsync(foodIds, ct)).ToList();

        var dishAsIngredient = foods.FirstOrDefault(f => f.IsDish);
        if (dishAsIngredient is not null)
            return (null, $"'{dishAsIngredient.Name}' è un piatto e non può essere usato come ingrediente.");

        var missingCount = foodIds.Except(foods.Select(f => f.Id)).Count();
        if (missingCount > 0)
            return (null, $"{missingCount} alimento/i non trovato/i.");

        var allNutrients = (await drRepository.GetNutrientsForFoodsAsync(foodIds, ct)).ToList();
        var totalWeight = dto.Ingredients.Sum(i => i.QuantityGrams);

        // Accumula contributi per (NutrientId, UnitOfMeasureId)
        var contributions = new Dictionary<(Guid NutrientId, Guid UomId), decimal>();
        foreach (var ingredient in dto.Ingredients)
        {
            foreach (var fn in allNutrients.Where(n => n.FoodId == ingredient.FoodId))
            {
                var key = (fn.NutrientId, fn.UnitOfMeasureId);
                contributions[key] = contributions.GetValueOrDefault(key) + fn.Quantity * (ingredient.QuantityGrams / 100m);
            }
        }

        var dishId = Guid.NewGuid();
        var dish = new Food
        {
            Id = dishId,
            Name = dto.Name,
            IsDish = true,
            Quantity = 100m,
            Calorie = Math.Round(
                dto.Ingredients.Sum(i => foods.First(f => f.Id == i.FoodId).Calorie * (i.QuantityGrams / 100m))
                / totalWeight * 100m, 2),
            BrandId = null,
            UnitOfMeasureId = Constants.GetDefaultUnitOfMeasure()
        };

        foreach (var kv in contributions)
        {
            dish.FoodsNutrients.Add(new FoodNutrient
            {
                FoodId = dishId,
                NutrientId = kv.Key.NutrientId,
                UnitOfMeasureId = kv.Key.UomId,
                Quantity = Math.Round(kv.Value / totalWeight * 100m, 2)
            });
        }

        var ingredients = dto.Ingredients.Select(i => new DishIngredient
        {
            DishId = dishId,
            FoodId = i.FoodId,
            QuantityGrams = i.QuantityGrams
        }).ToList();

        await drRepository.CreateDishAsync(dish, ingredients, ct).ConfigureAwait(false);
        return (dishId, null);
    }

    public async Task<IList<FoodDashboardInfo>> GetDishesDashboardAsync(CancellationToken ct = default) =>
        (await drRepository.GetDishesDashboardAsync(ct).ConfigureAwait(false)).ToList();

    public async Task DeleteDishAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.DeleteDishAsync(id, ct).ConfigureAwait(false);
}
