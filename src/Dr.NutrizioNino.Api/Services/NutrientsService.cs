using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services
{
    public class NutrientsService(INutrientsRepository nutrientsRepository)
    {
        public async Task<ApiResponseDto<NutrientDto>> GetNutrientsAsync()
        {
            var nutrient = await nutrientsRepository.GetNutrientsAsync().ConfigureAwait(false);

            return new ApiResponseDto<NutrientDto>()
            {
                Success = true,
                Data = nutrient.OrderBy(x => x.PositionOrder).Select(x => x.AsDto()).ToList()
            };
        }

        public async Task<Nutrient> GetNutrientAsync(Guid id)
        {
            return await nutrientsRepository.GetNutrientAsync(id);
        }

        public async Task<Nutrient> CreateNutrientAsync(CreateNutrientDto newNutrientDto)
        {
            var nutrient = await ModelsFactory.CreateNutrient(newNutrientDto);
            return await nutrientsRepository.CreateNutrientAsync(nutrient);
        }

        public async Task UpdateNutrientAsync(Nutrient nutrient)
        {
            await nutrientsRepository.UpdateNutrientAsync(nutrient);
        }

        public async Task DeleteBrandAsync(Guid id)
        {
            await nutrientsRepository.DeleteNutrientAsync(id);
        }

        public async Task<ApiResponseDto<NutrientsGetForFoodCreatingInfo>> GetNutrientsForFoodCreating() 
        {
            var nutrients = await nutrientsRepository.GetNutrientsForFoodCreatingAsync();

            return new ApiResponseDto<NutrientsGetForFoodCreatingInfo>
            {
                Success = true,
                Data = nutrients.OrderBy(x => x.PositionOrder).ToList()
            };
        }
    }
}
