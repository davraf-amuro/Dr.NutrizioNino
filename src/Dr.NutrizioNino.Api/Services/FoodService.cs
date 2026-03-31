using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class FoodService(DrRepository drRepository)
{
    public async Task<bool> IsFoodNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drRepository.IsFoodNameTakenAsync(name, excludeId, ct).ConfigureAwait(false);

    public async Task<bool> UpdateFoodAsync(Food food, CancellationToken ct = default) =>
        await drRepository.UpdateFoodAsync(food, ct).ConfigureAwait(false);

    public async Task<bool> UpdateFullFoodAsync(FoodInfo foodInfo, CancellationToken ct = default)
    {
        var food = new Food
        {
            Id = foodInfo.Id,
            Name = foodInfo.Name,
            Quantity = foodInfo.Quantity,
            Barcode = foodInfo.Barcode,
            BrandId = foodInfo.BrandId,
            Calorie = foodInfo.Calorie,
            UnitOfMeasureId = foodInfo.UnitOfMeasureId
        };

        foreach (var nutrient in foodInfo.Nutrients)
        {
            food.FoodsNutrients.Add(new FoodNutrient
            {
                FoodId = food.Id,
                NutrientId = nutrient.NutrientId,
                UnitOfMeasureId = nutrient.UnitOfMeasureId,
                Quantity = nutrient.Quantity
            });
        }

        foreach (var supermarketId in foodInfo.SupermarketIds ?? [])
        {
            food.FoodSupermarkets.Add(new FoodSupermarket
            {
                FoodId = food.Id,
                SupermarketId = supermarketId
            });
        }

        return await drRepository.UpdateFullFoodAsync(food, ct).ConfigureAwait(false);
    }

    public async Task DeleteFoodAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.DeleteFoodAsync(id, ct).ConfigureAwait(false);

    public async Task<IList<FoodDashboardInfo>> GetFoodsDashboardAsync(string? nameFilter = null, CancellationToken ct = default)
    {
        var result = await drRepository.GetFoodsDashboardAsync(nameFilter, ct).ConfigureAwait(false);
        return result.ToList();
    }

    public async Task<FoodDashboardInfo?> GetFoodDashboardAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetFoodDashboardAsync(id, ct).ConfigureAwait(false);

    public async Task<FoodInfo?> GetFullFood(Guid? id, CancellationToken ct = default)
    {
        var nutrients = (await drRepository.GetAllNutrientsForFood(id, ct)).ToList();
        var food = id.HasValue
            ? await drRepository.GetFoodAsync(id.Value, ct).ConfigureAwait(false)
            : null;

        if (id.HasValue && food is null)
        {
            return null;
        }

        var supermarketIds = food?.FoodSupermarkets.Select(fs => fs.SupermarketId).ToList();

        return new FoodInfo(
            food?.Id ?? Guid.Empty,
            food?.Name ?? "Empty Food",
            food?.Quantity ?? Constants.GetDefaultQuantity(),
            food?.Barcode,
            food?.BrandId ?? Constants.GetDefaultBrandId(),
            food?.Calorie ?? Constants.GetDefaultCalories(),
            food?.UnitOfMeasureId ?? Constants.GetDefaultUnitOfMeasure(),
            nutrients,
            supermarketIds);
    }

    public async Task<Guid> InsertFullFood(FoodInfo foodInfo, CancellationToken ct = default)
    {
        var food = new Food
        {
            Id = Guid.NewGuid(),
            Name = foodInfo.Name,
            Quantity = foodInfo.Quantity,
            Barcode = null,
            BrandId = foodInfo.BrandId,
            Calorie = foodInfo.Calorie,
            UnitOfMeasureId = foodInfo.UnitOfMeasureId
        };

        foreach (var nutrient in foodInfo.Nutrients)
        {
            food.FoodsNutrients.Add(new FoodNutrient
            {
                FoodId = food.Id,
                NutrientId = nutrient.NutrientId,
                UnitOfMeasureId = nutrient.UnitOfMeasureId,
                Quantity = nutrient.Quantity
            });
        }

        foreach (var supermarketId in foodInfo.SupermarketIds ?? [])
        {
            food.FoodSupermarkets.Add(new FoodSupermarket
            {
                FoodId = food.Id,
                SupermarketId = supermarketId
            });
        }

        await drRepository.InsertFullFood(food, ct).ConfigureAwait(false);
        return food.Id;
    }
}
