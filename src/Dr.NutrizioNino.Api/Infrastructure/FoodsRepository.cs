using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Infrastructure
{
    public class FoodsRepository : IFoodsRepository
    {
        public async Task<Food> CreateFoodAsync(CreateFoodDto newFoodDto)
        {
            var newFood = await ModelsFactory.CreateFood();

            return await Task.FromResult(newFood);
        }
        public async Task DeleteFoodAsync(Guid id)
        {
            throw new Exception("Not implemented yet!");
        }
        public async Task<Food> GetFoodAsync(Guid id)
        {
            return await Task.FromResult(new Food());
        }
        public async Task<IEnumerable<Food>> GetFoodsAsync()
        {
            return await Task.FromResult(new List<Food>());
        }
        public async Task UpdateFoodAsync(Food food)
        {
            throw new Exception("Not implemented yet!");
        }

    }
}
