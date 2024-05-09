using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;

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
        public async Task<IEnumerable<UnitOfMeasure>> GetUnitsOfMeasuresAsync()
        {
            return await Task.FromResult(new List<UnitOfMeasure>());
        }
        public async Task UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
        {
            throw new Exception("Not implemented yet!");
        }
    }
}
