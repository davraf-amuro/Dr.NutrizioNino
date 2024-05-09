using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Interfaces
{
    public interface IFoodsRepository
    {
        Task<Food> CreateFoodAsync(CreateFoodDto newFoodDto);
        Task DeleteFoodAsync(Guid id);
        Task<Food> GetFoodAsync(Guid id);
        Task<IEnumerable<Food>> GetFoodsAsync();
        Task UpdateFoodAsync(Food food);
    }
}
