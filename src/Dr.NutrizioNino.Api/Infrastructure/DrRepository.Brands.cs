using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public Task CreateBrandAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task DeleteBrandAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<Brand> GetBrandAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Brand>> GetBrandsAsync() => drContext.Brands.AsNoTracking();

    public Task UpdateBrandAsync(Brand brand)
    {
        throw new NotImplementedException();
    }
}
