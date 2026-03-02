using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public partial class DrService
{
    public async Task<IList<BrandDto>> GetBrandsAsync()
    {
        var brands = await drRepository.GetBrandsAsync().ConfigureAwait(false);
        return brands.Select(x => x.AsDto()).ToList();
    }

    public async Task<Brand?> GetBrandAsync(Guid id)
    {
        return await drRepository.GetBrandAsync(id);
    }

    public async Task<Brand> CreateBrandAsync(CreateBrandDto newBrandDto)
    {
        var brand = await ModelsFactory.CreateBrand(newBrandDto);
        return await drRepository.CreateBrandAsync(brand);
    }

    public async Task<bool> UpdateBrandAsync(Brand brand)
    {
        return await drRepository.UpdateBrandAsync(brand).ConfigureAwait(false);
    }

    public async Task<bool> DeleteBrandAsync(Guid id)
    {
        return await drRepository.DeleteBrandAsync(id).ConfigureAwait(false);
    }

}
