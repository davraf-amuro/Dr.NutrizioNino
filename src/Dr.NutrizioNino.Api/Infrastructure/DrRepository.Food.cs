using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<Food> CreateFoodAsync(CreateFoodDto newFoodDto, CancellationToken ct = default)
    {
        var newFood = await ModelsFactory.CreateFood(newFoodDto).ConfigureAwait(false);
        drContext.Foods.Add(newFood);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return newFood;
    }

    public async Task DeleteFoodAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.Foods
            .Include(f => f.FoodsNutrients)
            .Include(f => f.FoodSupermarkets)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            .ConfigureAwait(false);
        if (record is null)
        {
            return;
        }

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.FoodsNutrients.RemoveRange(record.FoodsNutrients);
        drContext.FoodSupermarkets.RemoveRange(record.FoodSupermarkets);
        drContext.Foods.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
    }

    public async Task<IList<TResult>> GetFoodsAsync<TResult>(
        Expression<Func<Food, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.Foods
            .AsNoTracking()
            .Select(selector)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<TResult?> GetFoodAsync<TResult>(
        Guid id,
        Expression<Func<Food, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.Foods
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    internal async Task<Food?> GetFoodAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Foods
            .AsNoTracking()
            .Include(f => f.FoodSupermarkets)
            .FirstOrDefaultAsync(x => x.Id == id, ct)
            .ConfigureAwait(false);

    public async Task<bool> UpdateFoodAsync(Food food, CancellationToken ct = default)
    {
        var record = await drContext.Foods.FindAsync(new object?[] { food.Id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        var calorieChanged = record.Calorie != food.Calorie;

        record.Name = food.Name;
        record.Quantity = food.Quantity;
        record.Barcode = food.Barcode;
        record.BrandId = food.BrandId;
        record.Calorie = food.Calorie;
        record.UnitOfMeasureId = food.UnitOfMeasureId;

        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);

        if (calorieChanged)
        {
            await MarkDishesStaleByFoodIdAsync(food.Id, ct).ConfigureAwait(false);
        }

        return true;
    }

    public async Task<IEnumerable<FoodDashboardInfo>> GetFoodsDashboardAsync(string? nameFilter = null, CancellationToken ct = default) =>
        await drContext.FoodsDashboard
            .AsNoTracking()
            .Where(f => nameFilter == null || EF.Functions.Like(f.Name!, $"%{nameFilter}%"))
            .ToListAsync(ct)
            .ConfigureAwait(false);

    internal async Task<FoodDashboardInfo?> GetFoodDashboardAsync(Guid id, CancellationToken ct = default) =>
        await drContext.FoodsDashboard
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            .ConfigureAwait(false);

    internal async Task<Guid> InsertFullFood(Food food, CancellationToken ct = default)
    {
        drContext.Foods.Add(food);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return food.Id;
    }

    public async Task<Guid?> GetFoodOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Foods
            .AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => f.OwnerId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task<bool> IsFoodNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drContext.Foods
            .AnyAsync(f => f.Name.ToLower() == name.ToLower() && (!excludeId.HasValue || f.Id != excludeId.Value), ct)
            .ConfigureAwait(false);

    public async Task<bool> UpdateFullFoodAsync(Food food, CancellationToken ct = default)
    {
        var record = await drContext.Foods
            .Include(f => f.FoodsNutrients)
            .Include(f => f.FoodSupermarkets)
            .FirstOrDefaultAsync(f => f.Id == food.Id, ct)
            .ConfigureAwait(false);

        if (record is null)
        {
            return false;
        }

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);

        var calorieChanged = record.Calorie != food.Calorie;
        var nutrientsChanged = record.FoodsNutrients.Count != food.FoodsNutrients.Count
            || record.FoodsNutrients.Any(existing => food.FoodsNutrients
                .All(n => n.NutrientId != existing.NutrientId || n.Quantity != existing.Quantity));

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

        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);

        if (calorieChanged || nutrientsChanged)
        {
            await MarkDishesStaleByFoodIdAsync(food.Id, ct).ConfigureAwait(false);
        }

        return true;
    }
}
