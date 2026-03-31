using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class BrandService(DrRepository drRepository)
{
    public async Task<IList<BrandDto>> GetBrandsAsync(CancellationToken ct = default)
    {
        var brands = await drRepository.GetBrandsAsync(ct).ConfigureAwait(false);
        return brands.Select(x => x.AsDto()).ToList();
    }

    public async Task<Brand?> GetBrandAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetBrandAsync(id, ct).ConfigureAwait(false);

    public async Task<Brand> CreateBrandAsync(CreateBrandDto newBrandDto, CancellationToken ct = default)
    {
        var brand = await ModelsFactory.CreateBrand(newBrandDto);
        return await drRepository.CreateBrandAsync(brand, ct).ConfigureAwait(false);
    }

    public async Task<bool> UpdateBrandAsync(Brand brand, CancellationToken ct = default) =>
        await drRepository.UpdateBrandAsync(brand, ct).ConfigureAwait(false);

    public async Task<bool> DeleteBrandAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.DeleteBrandAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsBrandInUseAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.IsBrandInUseAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsBrandNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drRepository.IsBrandNameTakenAsync(name, excludeId, ct).ConfigureAwait(false);
}
