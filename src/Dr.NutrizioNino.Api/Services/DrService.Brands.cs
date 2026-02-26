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

    public async Task<Brand> GetBrandAsync(Guid id)
    {
        return await drRepository.GetBrandAsync(id);
    }

    public async Task<Brand?> CreateBrandAsync(CreateBrandDto newBrandDto)
    {
        var brand = await ModelsFactory.CreateBrand(newBrandDto);
        return null; //await drRepository.CreateBrandAsync();
    }

    public async Task UpdateBrandAsync(Brand brand)
    {
        await drRepository.UpdateBrandAsync(brand);
    }

    //public async Task DeleteBrandAsync(Guid id)
    //{
    //    await drRepository.DeleteBrandAsync(id);
    //}

}
