using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services
{
    public partial class DrService
    {
        public async Task<ApiResponseMultipleDto<UnitOfMeasureDto>> GetUnitsOfMeasuresAsync()
        {
            var uom = await drRepository.GetUnitsOfMeasuresAsync().ConfigureAwait(false);

            return new ApiResponseMultipleDto<UnitOfMeasureDto>()
            {
                Success = true,
                Data = uom.Select(x => x.AsDto()).ToList()
            };
        }

        public async Task<UnitOfMeasure> GetUnitOfMeasureAsync(Guid id)
        {
            return await drRepository.GetUnitOfMeasureAsync(id).ConfigureAwait(false);
        }

        public async Task<UnitOfMeasure> CreateUnitOfMeasureAsync(CreateUnitOfMeasureDto newUnitOfMeasure)
        {
            var unitOfMeasure = await ModelsFactory.CreateUnitOfMeasure(newUnitOfMeasure).ConfigureAwait(false);
            return await drRepository.CreateUnitOfMeasureAsync(unitOfMeasure).ConfigureAwait(false);
        }

        public async Task<ApiResponseMultipleDto<UnitOfMeasureDto>> UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
        {
            var result = await drRepository.UpdateUnitOfMeasureAsync(unitOfMeasure).ConfigureAwait(false);
            var data = new List<UnitOfMeasureDto>();
            if (result != null) data.Add(result.AsDto());

            return new ApiResponseMultipleDto<UnitOfMeasureDto>
            {
                Success = (result != null),
                Data = data
            };
        }

        public async Task DeleteUnitOfMeasureAsync(Guid id)
        {
            await drRepository.DeleteUnitOfMeasureAsync(id).ConfigureAwait(false);
        }
    }
}
