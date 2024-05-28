using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services
{
    public class UnitsOfMeasuresService(IUnitOfMeasureRepository unitsOfMeasuresRepository)
    {
        public async Task<ApiResponseDto<UnitOfMeasureDto>> GetUnitsOfMeasuresAsync()
        {
            var uom = await unitsOfMeasuresRepository.GetUnitsOfMeasuresAsync().ConfigureAwait(false);

            return new ApiResponseDto<UnitOfMeasureDto>()
            {
                Success = true,
                Data = uom.Select(x => x.AsDto()).ToList()
            };
        }

        public async Task<UnitOfMeasure> GetUnitOfMeasureAsync(Guid id)
        {
            return await unitsOfMeasuresRepository.GetUnitOfMeasureAsync(id).ConfigureAwait(false);
        }

        public async Task<UnitOfMeasure> CreateUnitOfMeasureAsync(CreateUnitOfMeasureDto newUnitOfMeasure)
        {
            var unitOfMeasure = await ModelsFactory.CreateUnitOfMeasure(newUnitOfMeasure).ConfigureAwait(false);
            return await unitsOfMeasuresRepository.CreateUnitOfMeasureAsync(unitOfMeasure).ConfigureAwait(false);
        }

        public async Task<ApiResponseDto<UnitOfMeasureDto>> UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
        {
            var result = await unitsOfMeasuresRepository.UpdateUnitOfMeasureAsync(unitOfMeasure).ConfigureAwait(false);
            var data = new List<UnitOfMeasureDto>();
            if (result != null) data.Add(result.AsDto());

            return new ApiResponseDto<UnitOfMeasureDto>
            {
                Success = (result != null),
                Data = data
            };
        }

        public async Task DeleteUnitOfMeasureAsync(Guid id)
        {
            await unitsOfMeasuresRepository.DeleteUnitOfMeasureAsync(id).ConfigureAwait(false);
        }
    }
}
