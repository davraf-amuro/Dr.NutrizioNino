using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services
{
    public partial class DrService
    {
        public async Task<ApiResponseMultipleDto<NutrientInfo>> GetNutrientsAsync()
        {
            var nutrient = await drRepository.GetNutrientsAsync().ConfigureAwait(false);

            return new ApiResponseMultipleDto<NutrientInfo>()
            {
                Success = true,
                Data = nutrient.OrderBy(x => x.PositionOrder).Select(x => x.AsDto()).ToList()
            };
        }

        public async Task<Nutrient> GetNutrientAsync(Guid id)
        {
            return await drRepository.GetNutrientAsync(id);
        }

        public async Task<Nutrient> CreateNutrientAsync(CreateNutrientDto newNutrientDto)
        {
            var nutrient = await ModelsFactory.CreateNutrient(newNutrientDto);
            return await drRepository.CreateNutrientAsync(nutrient);
        }

        public async Task UpdateNutrientAsync(Nutrient nutrient)
        {
            await drRepository.UpdateNutrientAsync(nutrient);
        }

        public async Task DeleteBrandAsync(Guid id)
        {
            await drRepository.DeleteNutrientAsync(id);
        }

        //public async Task<ApiResponseDto<NutrientsGetForFoodCreatingInfo>> GetNutrientsForFoodCreating()
        //{
        //    var nutrients = await drRepository.GetNutrientsForFoodCreatingAsync();

        //    return new ApiResponseDto<NutrientsGetForFoodCreatingInfo>
        //    {
        //        Success = true,
        //        Data = nutrients.OrderBy(x => x.PositionOrder).ToList()
        //    };
        //}
    }
}
