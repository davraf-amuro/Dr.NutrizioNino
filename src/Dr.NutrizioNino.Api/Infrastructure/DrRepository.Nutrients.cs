using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<Nutrient> CreateNutrientAsync(Nutrient nutrient)
    {
        drContext.Nutrients.Add(nutrient);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return nutrient;
    }

    public async Task DeleteNutrientAsync(Guid id)
    {
        var record = await drContext.Nutrients.FindAsync(id).ConfigureAwait(false);
        if (record is null)
        {
            return;
        }

        drContext.Nutrients.Remove(record);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Nutrient>> GetNutrientsAsync() =>
        await drContext.Nutrients.AsNoTracking().ToListAsync().ConfigureAwait(false);

    public async Task<Nutrient?> GetNutrientAsync(Guid id)
    {
        return await drContext.Nutrients
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task UpdateNutrientAsync(Nutrient nutrient)
    {
        var record = await drContext.Nutrients.FindAsync(nutrient.Id).ConfigureAwait(false);
        if (record is null)
        {
            return;
        }

        record.Name = nutrient.Name;
        record.PositionOrder = nutrient.PositionOrder;
        record.DefaultQuantity = nutrient.DefaultQuantity;
        record.DefaultUnitOfMeasureId = nutrient.DefaultUnitOfMeasureId;

        await drContext.SaveChangesAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// restituisce tutti i nutrienti. Se riceve il guid di un cibo restituisce i nutrienti di quel cibo più quelli mancanti
    /// </summary>
    /// <param name="id">Guid del cibo</param>
    /// <returns></returns>
    public async Task<IEnumerable<NutrientsGetForFoodCreatingInfo>> GetAllNutrientsForFood(Guid? id)
    {
        var nutrients = await drContext.NutrientsGetForFoodCreatingInfoes
            .FromSql($"EXECUTE dbo.Full_Nutrients_For_Food {id}")
            .AsNoTracking()
            .ToListAsync()
            .ConfigureAwait(false);

        return nutrients;
    }

}
