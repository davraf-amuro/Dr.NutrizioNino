using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure
{
    public partial class DrRepository
    {
        public async Task<Food> CreateFoodAsync(CreateFoodDto newFoodDto)
        {
            //var newFood = await ModelsFactory.CreateFood(newFoodDto);
            //drContext.Foods.Add(newFood);
            //drContext.SaveChanges();
            //return await Task.FromResult(newFood);
            return await Task.FromResult(new Food());
        }
        public async Task DeleteFoodAsync(Guid id)
        {
            throw new Exception("Not implemented yet!");
        }

        public async Task<IEnumerable<Food>> GetFoodsAsync()
        {
            return drContext.Foods;
        }
        public async Task UpdateFoodAsync(Food food)
        {
            throw new Exception("Not implemented yet!");
        }

        public async Task<IEnumerable<FoodDashboardInfo>> GetFoodsDashboardAsync()
        {
            return drContext.FoodsDashboard;
        }

        internal async Task<FoodDashboardInfo?> GetFoodDashboardAsync(Guid id)
        {
            return await drContext.FoodsDashboard.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        internal async Task<Guid> InsertFullFood(Food food)
        {
            drContext.Foods.Add(food);
            await drContext.SaveChangesAsync();
            return food.Id;
        }
    }
}
