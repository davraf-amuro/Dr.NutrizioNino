using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class SupermarketService(DrRepository drRepository)
{
    public async Task<IList<SupermarketDto>> GetSupermarketsAsync(CancellationToken ct = default)
    {
        var supermarkets = await drRepository.GetSupermarketsAsync(ct).ConfigureAwait(false);
        return supermarkets.Select(s => new SupermarketDto(s.Id, s.Name)).ToList();
    }

    public async Task<Supermarket?> GetSupermarketAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetSupermarketAsync(id, ct).ConfigureAwait(false);

    public async Task<Supermarket> CreateSupermarketAsync(CreateSupermarketDto dto, CancellationToken ct = default)
    {
        var supermarket = new Supermarket { Id = Guid.NewGuid(), Name = dto.Name };
        return await drRepository.CreateSupermarketAsync(supermarket, ct).ConfigureAwait(false);
    }

    public async Task<bool> UpdateSupermarketAsync(Supermarket supermarket, CancellationToken ct = default) =>
        await drRepository.UpdateSupermarketAsync(supermarket, ct).ConfigureAwait(false);

    public async Task<bool> DeleteSupermarketAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.DeleteSupermarketAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsSupermarketInUseAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.IsSupermarketInUseAsync(id, ct).ConfigureAwait(false);

    public async Task<bool> IsSupermarketNameTakenAsync(string name, Guid? excludeId = null, CancellationToken ct = default) =>
        await drRepository.IsSupermarketNameTakenAsync(name, excludeId, ct).ConfigureAwait(false);
}
