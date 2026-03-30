using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class FoodService(DrRepository drRepository)
{
    public async Task<bool> IsFoodNameTakenAsync(string name, Guid? excludeId = null)
    {
        return await drRepository.IsFoodNameTakenAsync(name, excludeId).ConfigureAwait(false);
    }

    public async Task<Food> CreateFoodAsync(CreateFoodDto newFoodDto)
    {
        return await drRepository.CreateFoodAsync(newFoodDto);
    }

    public async Task<bool> UpdateFoodAsync(Food food)
    {
        return await drRepository.UpdateFoodAsync(food).ConfigureAwait(false);
    }

    public async Task<bool> UpdateFullFoodAsync(FoodInfo foodInfo)
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

        return await drRepository.UpdateFullFoodAsync(food).ConfigureAwait(false);
    }

    public async Task DeleteFoodAsync(Guid id)
    {
        await drRepository.DeleteFoodAsync(id);
    }

    public async Task<IList<FoodDashboardInfo>> GetFoodsDashboardAsync(string? nameFilter = null)
    {
        var request = await drRepository.GetFoodsDashboardAsync(nameFilter).ConfigureAwait(false);
        return request.ToList();
    }

    public async Task<FoodDashboardInfo?> GetFoodDashboardAsync(Guid id)
    {
        return await drRepository.GetFoodDashboardAsync(id).ConfigureAwait(false);
    }

    public async Task<FoodInfo?> GetFullFood(Guid? id)
    {
        var nutrients = (await drRepository.GetAllNutrientsForFood(id)).ToList();
        var food = id.HasValue
            ? await drRepository.GetFoodAsync(id.Value).ConfigureAwait(false)
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

    public async Task<Guid> InsertFullFood(FoodInfo foodInfo)
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

        await drRepository.InsertFullFood(food);
        return food.Id;
    }
}
