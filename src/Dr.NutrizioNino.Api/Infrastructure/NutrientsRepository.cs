using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure
{
    public class NutrientsRepository(DrNutrizioNinoContext nutrientsContest) : INutrientsRepository
    {
        public async Task<Nutrient> CreateNutrientAsync(Nutrient nutrient)
        {
            nutrientsContest.Nutrients.Add(nutrient);
            _ = nutrientsContest.SaveChanges();
            return nutrient;
        }

        public async Task DeleteNutrientAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Nutrient>> GetNutrientsAsync() => 
            nutrientsContest.Nutrients.AsNoTracking();

        public async Task<Nutrient> GetNutrientAsync(Guid id)
        {
            return await nutrientsContest.Nutrients.FindAsync(id).ConfigureAwait(false);
        }

        public async Task UpdateNutrientAsync(Nutrient nutrient)
        {
            throw new NotImplementedException();
        }

        
        public  async Task<IEnumerable<NutrientsGetForFoodCreatingInfo>> GetNutrientsForFoodCreatingAsync() =>
            nutrientsContest.NutrientsGetForFoodCreatingInfoes.AsNoTracking();
    }
}
