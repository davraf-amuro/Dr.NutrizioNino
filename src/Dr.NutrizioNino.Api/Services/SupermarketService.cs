using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class SupermarketService(DrRepository drRepository)
{
    public async Task<IList<SupermarketDto>> GetSupermarketsAsync()
    {
        var supermarkets = await drRepository.GetSupermarketsAsync().ConfigureAwait(false);
        return supermarkets.Select(s => new SupermarketDto(s.Id, s.Name)).ToList();
    }

    public async Task<Supermarket?> GetSupermarketAsync(Guid id) =>
        await drRepository.GetSupermarketAsync(id).ConfigureAwait(false);

    public async Task<Supermarket> CreateSupermarketAsync(CreateSupermarketDto dto)
    {
        var supermarket = new Supermarket { Id = Guid.NewGuid(), Name = dto.Name };
        return await drRepository.CreateSupermarketAsync(supermarket).ConfigureAwait(false);
    }

    public async Task<bool> UpdateSupermarketAsync(Supermarket supermarket) =>
        await drRepository.UpdateSupermarketAsync(supermarket).ConfigureAwait(false);

    public async Task<bool> DeleteSupermarketAsync(Guid id) =>
        await drRepository.DeleteSupermarketAsync(id).ConfigureAwait(false);

    public async Task<bool> IsSupermarketInUseAsync(Guid id) =>
        await drRepository.IsSupermarketInUseAsync(id).ConfigureAwait(false);

    public async Task<bool> IsSupermarketNameTakenAsync(string name, Guid? excludeId = null) =>
        await drRepository.IsSupermarketNameTakenAsync(name, excludeId).ConfigureAwait(false);
}
