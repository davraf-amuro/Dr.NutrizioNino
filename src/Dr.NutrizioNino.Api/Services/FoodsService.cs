using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services
{
    public class FoodsService(IFoodsRepository foodRepository)
    {
        public async Task<IEnumerable<Food>> GetFoodsAsync()
        {
            return await foodRepository.GetFoodsAsync();
        }

        public async Task<Food> GetFoodAsync(Guid id)
        {
            return await foodRepository.GetFoodAsync(id);
        }

        public async Task<Food> CreateFoodAsync(CreateFoodDto newFoodDto)
        {
            return await foodRepository.CreateFoodAsync(newFoodDto);
        }

        public async Task UpdateFoodAsync(Food food)
        {
            await foodRepository.UpdateFoodAsync(food);
        }

        public async Task DeleteFoodAsync(Guid id)
        {
            await foodRepository.DeleteFoodAsync(id);
        }
    }
}
