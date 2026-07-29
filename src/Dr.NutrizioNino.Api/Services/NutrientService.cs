using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Extensions;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public enum NutrientOperationResult { Success, NotFound, Conflict }

public enum NutrientCreateResult { Success, Conflict, InvalidUnitOfMeasure }

public class NutrientService(DrRepository drRepository)
{
    public async Task<IList<NutrientInfo>> GetNutrientsAsync(CancellationToken ct = default) =>
        (await drRepository.GetNutrientsAsync(NutrientExtensions.ToNutrientInfo, ct).ConfigureAwait(false))
            .OrderBy(x => x.PositionOrder == 0 ? int.MaxValue : x.PositionOrder)
            .ThenBy(x => x.Name)
            .ToList();

    public async Task<NutrientInfo?> GetNutrientAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetNutrientAsync(id, NutrientExtensions.ToNutrientInfo, ct).ConfigureAwait(false);

    /// <summary>Crea un nutriente previa validazione dell'unità di misura (obbligatoria ed esistente) e del nome univoco.</summary>
    public async Task<(NutrientCreateResult Result, NutrientInfo? Nutrient)> CreateNutrientAsync(CreateNutrientDto newNutrientDto, CancellationToken ct = default)
    {
        // UoM obbligatoria: il nutriente deve avere un'unità di misura canonica valida
        if (!newNutrientDto.DefaultUnitOfMeasureId.HasValue || newNutrientDto.DefaultUnitOfMeasureId.Value == Guid.Empty)
        {
            return (NutrientCreateResult.InvalidUnitOfMeasure, null);
        }

        // Verifica esistenza FK unità di misura prima dell'insert (no fiducia nel client)
        var uomExists = await drRepository.GetUnitOfMeasureAsync(
            newNutrientDto.DefaultUnitOfMeasureId.Value, u => u.Name, ct).ConfigureAwait(false) is not null;
        if (!uomExists)
        {
            return (NutrientCreateResult.InvalidUnitOfMeasure, null);
        }

        var exists = await drRepository.NutrientNameExistsAsync(newNutrientDto.Name, ct: ct).ConfigureAwait(false);
        if (exists)
        {
            return (NutrientCreateResult.Conflict, null);
        }

        var maxOrder = await drRepository.GetMaxNutrientPositionOrderAsync(ct).ConfigureAwait(false);
        var nutrientWithOrder = newNutrientDto with { PositionOrder = maxOrder + 1 };
        var nutrient = ModelsFactory.CreateNutrient(nutrientWithOrder);

        var created = await drRepository.CreateNutrientAsync(nutrient, ct).ConfigureAwait(false);
        return (NutrientCreateResult.Success, NutrientExtensions.ToNutrientInfo.Compile()(created));
    }

    public async Task<bool> ReorderNutrientsAsync(IList<NutrientReorderItem> items, CancellationToken ct = default)
    {
        await drRepository.ReorderNutrientsAsync(items, ct).ConfigureAwait(false);
        return true;
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
