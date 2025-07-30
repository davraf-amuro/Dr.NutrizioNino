using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;
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

        /// <summary>
        /// restituisce tutti i nutrienti. Se riceve il guid di un cibo restituisce i nutrienti di quel cibo più quelli mancanti
        /// </summary>
        /// <param name="id">Guid del cibo</param>
        /// <returns></returns>
        public async Task<IEnumerable<NutrientsGetForFoodCreatingInfo>> GetAllNutrientsForFood(Guid? id)
        {
            var nutrients = await drContext.NutrientsGetForFoodCreatingInfoes
                .FromSqlRaw($"EXECUTE dbo.Full_Nutrients_For_Food {id}")
                .AsNoTracking()
                .ToListAsync()
                .ConfigureAwait(false);

            return nutrients;
        }

    }
}
