using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services
{
    public class UnitsOfMeasuresService(IUnitOfMeasureRepository unitsOfMeasuresRepository)
    {
        public async Task<IEnumerable<UnitOfMeasure>> GetUnitsOfMeasuresAsync()
        {
            return await unitsOfMeasuresRepository.GetUnitsOfMeasuresAsync();
        }

        public async Task<UnitOfMeasure> GetUnitOfMeasureAsync(Guid id)
        {
            return await unitsOfMeasuresRepository.GetUnitOfMeasureAsync(id);
        }

        public async Task<UnitOfMeasure> CreateUnitOfMeasureAsync(CreateUnitOfMeasureDto newUnitOfMeasure)
        {
            var unitOfMeasure = await ModelsFactory.CreateUnitOfMeasure(newUnitOfMeasure);
            return await unitsOfMeasuresRepository.CreateUnitOfMeasureAsync(unitOfMeasure);
        }

        public async Task UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
        {
            await unitsOfMeasuresRepository.UpdateUnitOfMeasureAsync(unitOfMeasure);
        }

        public async Task DeleteUnitOfMeasureAsync(Guid id)
        {
            await unitsOfMeasuresRepository.DeleteUnitOfMeasureAsync(id);
        }
    }
}
