using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public partial class DrService
{
    public async Task<IList<NutrientInfo>> GetNutrientsAsync()
    {
        var nutrient = await drRepository.GetNutrientsAsync().ConfigureAwait(false);
        return nutrient.OrderBy(x => x.PositionOrder).Select(x => x.AsDto()).ToList();
    }

    public async Task<Nutrient?> GetNutrientAsync(Guid id)
    {
        return await drRepository.GetNutrientAsync(id).ConfigureAwait(false);
    }

    public async Task<Nutrient> CreateNutrientAsync(CreateNutrientDto newNutrientDto)
    {
        var nutrient = await ModelsFactory.CreateNutrient(newNutrientDto);
        return await drRepository.CreateNutrientAsync(nutrient);
    }

    public async Task<bool> UpdateNutrientAsync(Nutrient nutrient)
    {
        return await drRepository.UpdateNutrientAsync(nutrient).ConfigureAwait(false);
    }

    public async Task<bool> DeleteNutrientAsync(Guid id)
    {
        return await drRepository.DeleteNutrientAsync(id).ConfigureAwait(false);
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
