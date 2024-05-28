using Dr.NutrizioNino.Api.Models;

namespace Dr.NutrizioNino.Api.Interfaces
{
    public interface INutrientsRepository
    {
        Task<Nutrient> CreateNutrientAsync(Nutrient nutrient);
        Task DeleteNutrientAsync(Guid id);
        Task<Nutrient> GetNutrientAsync(Guid id);
        Task<IEnumerable<Nutrient>> GetNutrientsAsync();
        Task UpdateNutrientAsync(Nutrient nutrient);
    }
}
