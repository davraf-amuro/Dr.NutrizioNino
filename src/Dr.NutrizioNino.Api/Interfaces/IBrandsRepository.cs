using Dr.NutrizioNino.Api.Models;

namespace Dr.NutrizioNino.Api.Interfaces
{
    public interface IBrandsRepository
    {
        Task<Brand> CreateBrandAsync(Brand brand);
        Task DeleteBrandAsync(Guid id);
        Task<Brand> GetBrandAsync(Guid id);
        Task<IEnumerable<Brand>> GetBrandsAsync();
        Task UpdateBrandAsync(Brand brand);
    }
}
