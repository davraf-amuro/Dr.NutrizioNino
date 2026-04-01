using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<UnitOfMeasure> CreateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure, CancellationToken ct = default)
    {
        drContext.UnitsOfMeasures.Add(unitOfMeasure);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return unitOfMeasure;
    }

    public async Task<bool> DeleteUnitOfMeasureAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.UnitsOfMeasures.FindAsync(new object?[] { id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        drContext.UnitsOfMeasures.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<TResult?> GetUnitOfMeasureAsync<TResult>(
        Guid id,
        Expression<Func<UnitOfMeasure, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.UnitsOfMeasures
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(selector)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task<IList<TResult>> GetUnitsOfMeasuresAsync<TResult>(
        Expression<Func<UnitOfMeasure, TResult>> selector,
        CancellationToken ct = default) =>
        await drContext.UnitsOfMeasures
            .AsNoTracking()
            .Select(selector)
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<UnitOfMeasure?> UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure, CancellationToken ct = default)
    {
        var record = await drContext.UnitsOfMeasures.FindAsync(new object?[] { unitOfMeasure.Id }, ct).ConfigureAwait(false);
        if (record is null)
        {
            return null;
        }

        record.Name = unitOfMeasure.Name;
        record.Abbreviation = unitOfMeasure.Abbreviation;
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return record;
    }

    public async Task<bool> UomNameExistsAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drContext.UnitsOfMeasures
            .AsNoTracking()
            .AnyAsync(u => u.Name == name && (excludeId == null || u.Id != excludeId), ct)
            .ConfigureAwait(false);

    public async Task<bool> UomAbbreviationExistsAsync(string abbreviation, Guid? excludeId = null, CancellationToken ct = default) =>
        await drContext.UnitsOfMeasures
            .AsNoTracking()
            .AnyAsync(u => u.Abbreviation == abbreviation && (excludeId == null || u.Id != excludeId), ct)
            .ConfigureAwait(false);

    public async Task<bool> IsUomInUseAsync(Guid id, CancellationToken ct = default) =>
        await drContext.Foods.AsNoTracking().AnyAsync(f => f.UnitOfMeasureId == id, ct).ConfigureAwait(false)
        || await drContext.FoodsNutrients.AsNoTracking().AnyAsync(fn => fn.UnitOfMeasureId == id, ct).ConfigureAwait(false)
        || await drContext.Nutrients.AsNoTracking().AnyAsync(n => n.DefaultUnitOfMeasureId == id, ct).ConfigureAwait(false);
}
