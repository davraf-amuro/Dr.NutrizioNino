using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services
{
    public class BrandsService(IBrandsRepository brandsRepository)
    {
        public async Task<ApiResponseDto<BrandDto>> GetBrandsAsync()
        {
            var brands = await brandsRepository.GetBrandsAsync().ConfigureAwait(false);

            return new ApiResponseDto<BrandDto>()
            {
                Success = true,
                Data = brands.Select(x => x.AsDto()).ToList()
            };
        }

        public async Task<Brand> GetBrandAsync(Guid id)
        {
            return await brandsRepository.GetBrandAsync(id);
        }

        public async Task<Brand> CreateBrandAsync(CreateBrandDto newBrandDto)
        {
            var brand = await ModelsFactory.CreateBrand(newBrandDto);
            return await brandsRepository.CreateBrandAsync(brand);
        }

        public async Task UpdateBrandAsync(Brand brand)
        {
            await brandsRepository.UpdateBrandAsync(brand);
        }

        public async Task DeleteBrandAsync(Guid id)
        {
            await brandsRepository.DeleteBrandAsync(id);
        }

    }
}
