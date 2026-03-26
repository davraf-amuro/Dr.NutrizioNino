using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<Food> CreateFoodAsync(CreateFoodDto newFoodDto)
    {
        var newFood = await ModelsFactory.CreateFood(newFoodDto).ConfigureAwait(false);
        drContext.Foods.Add(newFood);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return newFood;
    }

    public async Task DeleteFoodAsync(Guid id)
    {
        var record = await drContext.Foods
            .Include(f => f.FoodsNutrients)
            .Include(f => f.FoodSupermarkets)
            .FirstOrDefaultAsync(f => f.Id == id)
            .ConfigureAwait(false);
        if (record is null)
        {
            return;
        }

        await using var transaction = await drContext.Database.BeginTransactionAsync().ConfigureAwait(false);
        drContext.FoodsNutrients.RemoveRange(record.FoodsNutrients);
        drContext.FoodSupermarkets.RemoveRange(record.FoodSupermarkets);
        drContext.Foods.Remove(record);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        await transaction.CommitAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Food>> GetFoodsAsync() =>
        await drContext.Foods.AsNoTracking().ToListAsync().ConfigureAwait(false);

    internal async Task<Food?> GetFoodAsync(Guid id)
    {
        return await drContext.Foods
            .AsNoTracking()
            .Include(f => f.FoodSupermarkets)
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<bool> UpdateFoodAsync(Food food)
    {
        var record = await drContext.Foods.FindAsync(food.Id).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        record.Name = food.Name;
        record.Quantity = food.Quantity;
        record.Barcode = food.Barcode;
        record.BrandId = food.BrandId;
        record.Calorie = food.Calorie;
        record.UnitOfMeasureId = food.UnitOfMeasureId;

        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<IEnumerable<FoodDashboardInfo>> GetFoodsDashboardAsync() =>
        await drContext.FoodsDashboard.AsNoTracking().ToListAsync().ConfigureAwait(false);

    internal async Task<FoodDashboardInfo?> GetFoodDashboardAsync(Guid id)
    {
        return await drContext.FoodsDashboard.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    }

    internal async Task<Guid> InsertFullFood(Food food)
    {
        drContext.Foods.Add(food);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return food.Id;
    }

    public async Task<bool> IsFoodNameTakenAsync(string name, Guid? excludeId = null) =>
        await drContext.Foods
            .AnyAsync(f => f.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || f.Id != excludeId.Value))
            .ConfigureAwait(false);

    public async Task<bool> UpdateFullFoodAsync(Food food)
    {
        var record = await drContext.Foods
            .Include(f => f.FoodsNutrients)
            .Include(f => f.FoodSupermarkets)
            .FirstOrDefaultAsync(f => f.Id == food.Id)
            .ConfigureAwait(false);

        if (record is null)
        {
            return false;
        }

        await using var transaction = await drContext.Database.BeginTransactionAsync().ConfigureAwait(false);

        record.Name = food.Name;
        record.Quantity = food.Quantity;
        record.Barcode = food.Barcode;
        record.BrandId = food.BrandId;
        record.Calorie = food.Calorie;
        record.UnitOfMeasureId = food.UnitOfMeasureId;

        drContext.FoodsNutrients.RemoveRange(record.FoodsNutrients);
        drContext.FoodsNutrients.AddRange(food.FoodsNutrients);

        drContext.FoodSupermarkets.RemoveRange(record.FoodSupermarkets);
        drContext.FoodSupermarkets.AddRange(food.FoodSupermarkets);

        await drContext.SaveChangesAsync().ConfigureAwait(false);
        await transaction.CommitAsync().ConfigureAwait(false);
        return true;
    }
}
