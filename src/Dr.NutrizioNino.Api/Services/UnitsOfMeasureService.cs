using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public enum UomOperationResult { Success, NotFound, DuplicateName, DuplicateAbbreviation, InUse }

public class UnitsOfMeasureService(DrRepository drRepository)
{
    public async Task<IList<UnitOfMeasureDto>> GetUnitsOfMeasuresAsync(CancellationToken ct = default)
    {
        var uom = await drRepository.GetUnitsOfMeasuresAsync(ct).ConfigureAwait(false);
        return uom.Select(x => x.AsDto()).ToList();
    }

    public async Task<UnitOfMeasure?> GetUnitOfMeasureAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetUnitOfMeasureAsync(id, ct).ConfigureAwait(false);

    public async Task<(UomOperationResult Result, UnitOfMeasure? Entity)> CreateUnitOfMeasureAsync(CreateUnitOfMeasureDto newUnitOfMeasure, CancellationToken ct = default)
    {
        if (await drRepository.UomNameExistsAsync(newUnitOfMeasure.Name, ct: ct).ConfigureAwait(false))
        {
            return (UomOperationResult.DuplicateName, null);
        }

        if (await drRepository.UomAbbreviationExistsAsync(newUnitOfMeasure.Abbreviation, ct: ct).ConfigureAwait(false))
        {
            return (UomOperationResult.DuplicateAbbreviation, null);
        }

        var unitOfMeasure = await ModelsFactory.CreateUnitOfMeasure(newUnitOfMeasure).ConfigureAwait(false);
        var entity = await drRepository.CreateUnitOfMeasureAsync(unitOfMeasure, ct).ConfigureAwait(false);
        return (UomOperationResult.Success, entity);
    }

    public async Task<UomOperationResult> UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure, CancellationToken ct = default)
    {
        if (await drRepository.UomNameExistsAsync(unitOfMeasure.Name, unitOfMeasure.Id, ct).ConfigureAwait(false))
        {
            return UomOperationResult.DuplicateName;
        }

        if (await drRepository.UomAbbreviationExistsAsync(unitOfMeasure.Abbreviation, unitOfMeasure.Id, ct).ConfigureAwait(false))
        {
            return UomOperationResult.DuplicateAbbreviation;
        }

        var result = await drRepository.UpdateUnitOfMeasureAsync(unitOfMeasure, ct).ConfigureAwait(false);
        return result is not null ? UomOperationResult.Success : UomOperationResult.NotFound;
    }

    public async Task<UomOperationResult> DeleteUnitOfMeasureAsync(Guid id, CancellationToken ct = default)
    {
        if (await drRepository.IsUomInUseAsync(id, ct).ConfigureAwait(false))
        {
            return UomOperationResult.InUse;
        }

        var deleted = await drRepository.DeleteUnitOfMeasureAsync(id, ct).ConfigureAwait(false);
        return deleted ? UomOperationResult.Success : UomOperationResult.NotFound;
    }
}
