using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public enum NutrientOperationResult { Success, NotFound, Conflict }

public class NutrientService(DrRepository drRepository)
{
    public async Task<IList<NutrientInfo>> GetNutrientsAsync()
    {
        var nutrient = await drRepository.GetNutrientsAsync().ConfigureAwait(false);
        return nutrient.OrderBy(x => x.Name).Select(x => x.AsDto()).ToList();
    }

    public async Task<Nutrient?> GetNutrientAsync(Guid id)
    {
        return await drRepository.GetNutrientAsync(id).ConfigureAwait(false);
    }

    public async Task<Nutrient?> CreateNutrientAsync(CreateNutrientDto newNutrientDto)
    {
        var exists = await drRepository.NutrientNameExistsAsync(newNutrientDto.Name).ConfigureAwait(false);
        if (exists)
        {
            return null;
        }

        var nutrient = await ModelsFactory.CreateNutrient(newNutrientDto);
        return await drRepository.CreateNutrientAsync(nutrient);
    }

    public async Task<NutrientOperationResult> UpdateNutrientAsync(Nutrient nutrient)
    {
        var duplicate = await drRepository.NutrientNameExistsAsync(nutrient.Name, nutrient.Id).ConfigureAwait(false);
        if (duplicate)
        {
            return NutrientOperationResult.Conflict;
        }

        var updated = await drRepository.UpdateNutrientAsync(nutrient).ConfigureAwait(false);
        return updated ? NutrientOperationResult.Success : NutrientOperationResult.NotFound;
    }

    public async Task<NutrientOperationResult> DeleteNutrientAsync(Guid id)
    {
        var inUse = await drRepository.IsNutrientInUseAsync(id).ConfigureAwait(false);
        if (inUse)
        {
            return NutrientOperationResult.Conflict;
        }

        var deleted = await drRepository.DeleteNutrientAsync(id).ConfigureAwait(false);
        return deleted ? NutrientOperationResult.Success : NutrientOperationResult.NotFound;
    }
}
