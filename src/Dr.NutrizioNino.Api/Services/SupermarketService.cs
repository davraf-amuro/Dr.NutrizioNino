using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Extensions;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class SupermarketService(DrRepository drRepository)
{
    public async Task<IList<SupermarketDto>> GetSupermarketsAsync(CancellationToken ct = default) =>
        await drRepository.GetSupermarketsAsync(SupermarketExtensions.ToSupermarketDto, ct).ConfigureAwait(false);

    public async Task<SupermarketDto?> GetSupermarketAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetSupermarketAsync(id, SupermarketExtensions.ToSupermarketDto, ct).ConfigureAwait(false);

    public async Task<SupermarketDto> CreateSupermarketAsync(CreateSupermarketDto dto, CancellationToken ct = default)
    {
        var supermarket = new Supermarket { Id = Guid.NewGuid(), Name = dto.Name };
        var created = await drRepository.CreateSupermarketAsync(supermarket, ct).ConfigureAwait(false);
        return SupermarketExtensions.ToSupermarketDto.Compile()(created);
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
