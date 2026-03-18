using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public enum UomOperationResult { Success, NotFound, DuplicateName, DuplicateAbbreviation, InUse }

public partial class DrService
{
    public async Task<IList<UnitOfMeasureDto>> GetUnitsOfMeasuresAsync()
    {
        var uom = await drRepository.GetUnitsOfMeasuresAsync().ConfigureAwait(false);
        return uom.Select(x => x.AsDto()).ToList();
    }

    public async Task<UnitOfMeasure?> GetUnitOfMeasureAsync(Guid id)
    {
        return await drRepository.GetUnitOfMeasureAsync(id).ConfigureAwait(false);
    }

    public async Task<(UomOperationResult Result, UnitOfMeasure? Entity)> CreateUnitOfMeasureAsync(CreateUnitOfMeasureDto newUnitOfMeasure)
    {
        if (await drRepository.UomNameExistsAsync(newUnitOfMeasure.Name).ConfigureAwait(false))
            return (UomOperationResult.DuplicateName, null);
        if (await drRepository.UomAbbreviationExistsAsync(newUnitOfMeasure.Abbreviation).ConfigureAwait(false))
            return (UomOperationResult.DuplicateAbbreviation, null);

        var unitOfMeasure = await ModelsFactory.CreateUnitOfMeasure(newUnitOfMeasure).ConfigureAwait(false);
        var entity = await drRepository.CreateUnitOfMeasureAsync(unitOfMeasure).ConfigureAwait(false);
        return (UomOperationResult.Success, entity);
    }

    public async Task<UomOperationResult> UpdateUnitOfMeasureAsync(UnitOfMeasure unitOfMeasure)
    {
        if (await drRepository.UomNameExistsAsync(unitOfMeasure.Name, unitOfMeasure.Id).ConfigureAwait(false))
            return UomOperationResult.DuplicateName;
        if (await drRepository.UomAbbreviationExistsAsync(unitOfMeasure.Abbreviation, unitOfMeasure.Id).ConfigureAwait(false))
            return UomOperationResult.DuplicateAbbreviation;

        var result = await drRepository.UpdateUnitOfMeasureAsync(unitOfMeasure).ConfigureAwait(false);
        return result is not null ? UomOperationResult.Success : UomOperationResult.NotFound;
    }

    public async Task<UomOperationResult> DeleteUnitOfMeasureAsync(Guid id)
    {
        if (await drRepository.IsUomInUseAsync(id).ConfigureAwait(false))
            return UomOperationResult.InUse;

        var deleted = await drRepository.DeleteUnitOfMeasureAsync(id).ConfigureAwait(false);
        return deleted ? UomOperationResult.Success : UomOperationResult.NotFound;
    }
}
