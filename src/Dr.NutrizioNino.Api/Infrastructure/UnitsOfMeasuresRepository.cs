using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure
{
    public class UnitsOfMeasuresRepository(DrNutrizioNinoContext context) : IUnitOfMeasureRepository
    {
        public async Task<UnitOfMeasure> CreateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
        {
            context.UnitsOfMeasures.Add(unitOfMeasure);
            _ = context.SaveChanges();
            return await Task.FromResult(unitOfMeasure);
        }
        public async Task DeleteUnitOfMeasureAsync(Guid id)
        {
            throw new Exception("Not implemented yet!");
        }
        public async Task<UnitOfMeasure> GetUnitOfMeasureAsync(Guid id)
        {
            return await Task.FromResult(new UnitOfMeasure());
        }
        public async Task<IEnumerable<UnitOfMeasure>> GetUnitsOfMeasuresAsync() => context.UnitsOfMeasures.AsNoTracking();

        public async Task<UnitOfMeasure?> UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
        {
            UnitOfMeasure response = null;

            var record = await context.UnitsOfMeasures.FindAsync(unitOfMeasure.Id).ConfigureAwait(false);
            if (record != null)
            {
                record.Name = unitOfMeasure.Name;
                record.Abbreviation = unitOfMeasure.Abbreviation;
                await context.SaveChangesAsync().ConfigureAwait(false);
                response = record;
            }

            return response;
        }
    }
}
