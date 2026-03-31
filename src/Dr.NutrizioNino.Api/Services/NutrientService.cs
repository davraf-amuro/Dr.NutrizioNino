using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public enum NutrientOperationResult { Success, NotFound, Conflict }

public class NutrientService(DrRepository drRepository)
{
    public async Task<IList<NutrientInfo>> GetNutrientsAsync(CancellationToken ct = default)
    {
        var nutrients = await drRepository.GetNutrientsAsync(ct).ConfigureAwait(false);
        return nutrients.OrderBy(x => x.Name).Select(x => x.AsDto()).ToList();
    }

    public async Task<Nutrient?> GetNutrientAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetNutrientAsync(id, ct).ConfigureAwait(false);

    public async Task<Nutrient?> CreateNutrientAsync(CreateNutrientDto newNutrientDto, CancellationToken ct = default)
    {
        var exists = await drRepository.NutrientNameExistsAsync(newNutrientDto.Name, ct: ct).ConfigureAwait(false);
        if (exists)
        {
            return null;
        }

        var nutrient = await ModelsFactory.CreateNutrient(newNutrientDto);
        return await drRepository.CreateNutrientAsync(nutrient, ct).ConfigureAwait(false);
    }

    public async Task<NutrientOperationResult> UpdateNutrientAsync(Nutrient nutrient, CancellationToken ct = default)
    {
        var duplicate = await drRepository.NutrientNameExistsAsync(nutrient.Name, nutrient.Id, ct).ConfigureAwait(false);
        if (duplicate)
        {
            return NutrientOperationResult.Conflict;
        }

        var updated = await drRepository.UpdateNutrientAsync(nutrient, ct).ConfigureAwait(false);
        return updated ? NutrientOperationResult.Success : NutrientOperationResult.NotFound;
    }

    public async Task<NutrientOperationResult> DeleteNutrientAsync(Guid id, CancellationToken ct = default)
    {
        var inUse = await drRepository.IsNutrientInUseAsync(id, ct).ConfigureAwait(false);
        if (inUse)
        {
            return NutrientOperationResult.Conflict;
        }

        var deleted = await drRepository.DeleteNutrientAsync(id, ct).ConfigureAwait(false);
        return deleted ? NutrientOperationResult.Success : NutrientOperationResult.NotFound;
    }
}
