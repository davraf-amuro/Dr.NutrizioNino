using Dr.NutrizioNino.Api.Models;

namespace Dr.NutrizioNino.Api.Interfaces
{
    public interface IUnitOfMeasureRepository
    {
        Task<UnitOfMeasure> CreateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure);
        Task DeleteUnitOfMeasureAsync(Guid id);
        Task<UnitOfMeasure> GetUnitOfMeasureAsync(Guid id);
        Task<IEnumerable<UnitOfMeasure>> GetUnitsOfMeasuresAsync();
        Task UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure);
    }
}
