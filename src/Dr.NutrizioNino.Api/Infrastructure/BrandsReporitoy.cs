using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure
{
    public class BrandsReporitoy(DrNutrizioNinoContext brandsContext) : IBrandsRepository
    {
        public Task<Brand> CreateBrandAsync(Brand brand)
        {
            brandsContext.Brands.Add(brand);
            _ = brandsContext.SaveChanges();
            return Task.FromResult(brand);
        }

        public Task DeleteBrandAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Brand> GetBrandAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Brand>> GetBrandsAsync()
        {
            return await brandsContext.Brands.ToListAsync();
        }

        public Task UpdateBrandAsync(Brand brand)
        {
            throw new NotImplementedException();
        }
    }
}
