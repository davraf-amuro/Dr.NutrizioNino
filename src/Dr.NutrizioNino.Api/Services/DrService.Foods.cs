using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services
{
    public partial class DrService
    {
        //public async Task<Food> GetFoodAsync(Guid id)
        //{
        //    return await drRepository.GetFoodAsync(id);
        //}

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

        public async Task<ApiResponseMultipleDto<FoodDashboardInfo>> GetFoodsDashboardAsync()
        {
            var request = await drRepository.GetFoodsDashboardAsync().ConfigureAwait(false);
            return new ApiResponseMultipleDto<FoodDashboardInfo>
            {
                Success = true,
                Data = request.ToList()
            };
        }

        public async Task<ApiResponseSingleDto<FoodDashboardInfo>> GetFoodDashboardAsync(Guid id)
        {
            var request = await drRepository.GetFoodDashboardAsync(id).ConfigureAwait(false);
            return new ApiResponseSingleDto<FoodDashboardInfo>
            {
                Success = true,
                Data = request
            };
        }

        public async Task<ApiResponseSingleDto<FoodInfo>> GetFullFood(Guid? id)
        {
            var nutrients = (await drRepository.GetAllNutrientsForFood(id)).ToList();
            var foodCreationTemplateDto = new FoodInfo(
                Guid.Empty
                , "Empty Food"
                , Constants.GetDefaultQuantity()
                , null
                , Constants.GetDefaultBrandId()
                , Constants.GetDefaultCalories()
                , Constants.GetDefaultUnitOfMeasure()
                , nutrients
                );

            return new ApiResponseSingleDto<FoodInfo>
            {
                Success = true,
                Data = foodCreationTemplateDto
            };
        }
    }
}

