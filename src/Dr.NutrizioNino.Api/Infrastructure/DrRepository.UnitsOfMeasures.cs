using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<UnitOfMeasure> CreateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
    {
        drContext.UnitsOfMeasures.Add(unitOfMeasure);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return unitOfMeasure;
    }

    public async Task<bool> DeleteUnitOfMeasureAsync(Guid id)
    {
        var record = await drContext.UnitsOfMeasures.FindAsync(id).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        drContext.UnitsOfMeasures.Remove(record);
        await drContext.SaveChangesAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<UnitOfMeasure?> GetUnitOfMeasureAsync(Guid id)
    {
        return await drContext.UnitsOfMeasures
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id)
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<UnitOfMeasure>> GetUnitsOfMeasuresAsync() =>
        await drContext.UnitsOfMeasures.AsNoTracking().ToListAsync().ConfigureAwait(false);

    public async Task<UnitOfMeasure?> UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
    {
        UnitOfMeasure? response = null;

        var record = await drContext.UnitsOfMeasures.FindAsync(unitOfMeasure.Id).ConfigureAwait(false);
        if (record != null)
        {
            record.Name = unitOfMeasure.Name;
            record.Abbreviation = unitOfMeasure.Abbreviation;
            await drContext.SaveChangesAsync().ConfigureAwait(false);
            response = record;
        }

        return response;
    }

    public async Task<bool> UomNameExistsAsync(string name, Guid? excludeId = null) =>
        await drContext.UnitsOfMeasures
            .AnyAsync(u => u.Name == name && (excludeId == null || u.Id != excludeId))
            .ConfigureAwait(false);

    public async Task<bool> UomAbbreviationExistsAsync(string abbreviation, Guid? excludeId = null) =>
        await drContext.UnitsOfMeasures
            .AnyAsync(u => u.Abbreviation == abbreviation && (excludeId == null || u.Id != excludeId))
            .ConfigureAwait(false);

    public async Task<bool> IsUomInUseAsync(Guid id) =>
        await drContext.Foods.AnyAsync(f => f.UnitOfMeasureId == id).ConfigureAwait(false)
        || await drContext.FoodsNutrients.AnyAsync(fn => fn.UnitOfMeasureId == id).ConfigureAwait(false)
        || await drContext.Nutrients.AnyAsync(n => n.DefaultUnitOfMeasureId == id).ConfigureAwait(false);
}
