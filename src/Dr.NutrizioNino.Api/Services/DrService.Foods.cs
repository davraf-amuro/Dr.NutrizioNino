using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services
{
    public partial class DrService
    {
        public async Task<ApiResponseDto<FoodDto>> GetFoodsAsync()
        {
            var request = await drRepository.GetFoodsAsync().ConfigureAwait(false);
            return new ApiResponseDto<FoodDto>
            {
                Success = true,
                Data = request.Select(x => x.AsDto()).ToList()
            };
        }

        public async Task<Food> GetFoodAsync(Guid id)
        {
            return await drRepository.GetFoodAsync(id);
        }

        public async Task<Food> CreateFoodAsync(CreateFoodDto newFoodDto)
        {
            return await drRepository.CreateFoodAsync(newFoodDto);
        }

        public async Task UpdateFoodAsync(Food food)
        {
            await drRepository.UpdateFoodAsync(food);
        }

        public async Task DeleteFoodAsync(Guid id)
        {
            await drRepository.DeleteFoodAsync(id);
        }

        public async Task<ApiResponseDto<FoodDashboard>> GetFoodsDashboardAsync()
        {
            var request = await drRepository.GetFoodsDashboardAsync().ConfigureAwait(false);
            return new ApiResponseDto<FoodDashboard>
            {
                Success = true,
                Data = request.ToList()
            };
        }

        public async Task<ApiResponseDto<FoodCreationTemplateDto>> FactoryGetNew()
        {
            var nutrients = (await drRepository.GetNutrientsAsync()).Select(x => x.AsDto()).ToList();
            var foodCreationTemplateDto = new FoodCreationTemplateDto(
                Guid.Empty
                , "Default Food"
                , Constants.GetDefaultQuantity()
                , null
                , Guid.Empty
                , Constants.GetDefaultCaolires()
                , nutrients
                );

            return new ApiResponseDto<FoodCreationTemplateDto>
            {
                Success = true,
                Data = new List<FoodCreationTemplateDto> { foodCreationTemplateDto }
            };
        }
    }
}

