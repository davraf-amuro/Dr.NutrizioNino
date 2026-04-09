using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class DailySimulationSectionService(DrRepository drRepository)
{
    public async Task<IList<SimulationSectionDto>> GetAllAsync(CancellationToken ct = default) =>
        await drRepository.GetSectionsAsync(ct).ConfigureAwait(false);

    public async Task<IList<SimulationSectionDto>> GetActiveAsync(CancellationToken ct = default) =>
        await drRepository.GetActiveSectionsAsync(ct).ConfigureAwait(false);

    public async Task<Guid> CreateAsync(string name, CancellationToken ct = default)
    {
        var maxOrder = await drRepository.GetSectionsAsync(ct).ConfigureAwait(false);
        var nextOrder = maxOrder.Count > 0 ? maxOrder.Max(s => s.DisplayOrder) + 1 : 1;

        var section = new DailySimulationSection
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            DisplayOrder = nextOrder,
            IsActive = true
        };
        return await drRepository.CreateSectionAsync(section, ct).ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(Guid id, string name, CancellationToken ct = default) =>
        await drRepository.UpdateSectionAsync(id, name.Trim(), ct).ConfigureAwait(false);

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.SoftDeleteSectionAsync(id, ct).ConfigureAwait(false);

    public async Task ReorderAsync(IList<SimulationSectionReorderItem> items, CancellationToken ct = default) =>
        await drRepository.ReorderSectionsAsync(items, ct).ConfigureAwait(false);
}
