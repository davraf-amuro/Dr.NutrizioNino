using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Extensions;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class BrandService(DrRepository drRepository)
{
    public async Task<IList<BrandDto>> GetBrandsAsync(CancellationToken ct = default) =>
        await drRepository.GetBrandsAsync(BrandExtensions.ToBrandDto, ct).ConfigureAwait(false);

    public async Task<IList<TResult>> GetBrandsAsync<TResult>(
        Expression<Func<Brand, TResult>> selector,
        CancellationToken ct = default) =>
        await drRepository.GetBrandsAsync(selector, ct).ConfigureAwait(false);

    public async Task<BrandDto?> GetBrandAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetBrandAsync(id, BrandExtensions.ToBrandDto, ct).ConfigureAwait(false);

    public async Task<BrandDto> CreateBrandAsync(CreateBrandDto newBrandDto, Guid? ownerId = null, CancellationToken ct = default)
    {
        var brand = await ModelsFactory.CreateBrand(newBrandDto);
        brand.OwnerId = ownerId;
        var created = await drRepository.CreateBrandAsync(brand, ct).ConfigureAwait(false);
        return BrandExtensions.ToBrandDto.Compile()(created);
    }

    public async Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetBrandOwnerIdAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> UpdateBrandAsync(Brand brand, CancellationToken ct = default) =>
        await drRepository.UpdateBrandAsync(brand, ct).ConfigureAwait(false);

    public async Task<bool> DeleteBrandAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.DeleteBrandAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsBrandInUseAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.IsBrandInUseAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsBrandNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drRepository.IsBrandNameTakenAsync(name, excludeId, ct).ConfigureAwait(false);
}
