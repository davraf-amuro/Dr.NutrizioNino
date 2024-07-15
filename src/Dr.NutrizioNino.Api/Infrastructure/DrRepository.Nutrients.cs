using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure
{
    public partial class DrRepository
    {
        public async Task<Nutrient> CreateNutrientAsync(Nutrient nutrient)
        {
            drContext.Nutrients.Add(nutrient);
            _ = drContext.SaveChanges();
            return nutrient;
        }

        public async Task DeleteNutrientAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Nutrient>> GetNutrientsAsync() =>
            drContext.Nutrients.AsNoTracking();

        public async Task<Nutrient> GetNutrientAsync(Guid id)
        {
            return await drContext.Nutrients.FindAsync(id).ConfigureAwait(false);
        }

        public async Task UpdateNutrientAsync(Nutrient nutrient)
        {
            throw new NotImplementedException();
        }


        //public async Task<IEnumerable<NutrientsGetForFoodCreatingInfo>> GetNutrientsForFoodCreatingAsync() =>
        //    drContext.NutrientsGetForFoodCreatingInfoes.AsNoTracking();
    }
}
