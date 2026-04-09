using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class DailySimulationService(DrRepository drRepository)
{
    public async Task<IList<DailySimulationListItemDto>> GetUserSimulationsAsync(Guid userId, CancellationToken ct = default) =>
        await drRepository.GetUserSimulationsAsync(userId, ct).ConfigureAwait(false);

    public async Task<DailySimulationDetailDto?> GetSimulationDetailAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetSimulationDetailAsync(id, ct).ConfigureAwait(false);

    public async Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.GetSimulationOwnerIdAsync(id, ct).ConfigureAwait(false);

    public async Task<Guid> CreateSimulationAsync(CreateDailySimulationDto dto, Guid userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var simulation = new DailySimulation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = dto.Name.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };
        return await drRepository.CreateSimulationAsync(simulation, ct).ConfigureAwait(false);
    }

    public async Task<bool> RenameSimulationAsync(Guid id, string name, CancellationToken ct = default) =>
        await drRepository.RenameSimulationAsync(id, name.Trim(), ct).ConfigureAwait(false);

    public async Task DeleteSimulationAsync(Guid id, CancellationToken ct = default) =>
        await drRepository.DeleteSimulationAsync(id, ct).ConfigureAwait(false);

    public async Task<Guid> CloneSimulationAsync(Guid id, Guid userId, CancellationToken ct = default)
    {
        var detail = await drRepository.GetSimulationDetailAsync(id, ct).ConfigureAwait(false);
        var sourceName = detail?.Name ?? "copia";
        return await drRepository.CloneSimulationAsync(id, userId, $"{sourceName} (copia)", ct).ConfigureAwait(false);
    }

    public async Task<(Guid? EntryId, string? Error)> AddEntryAsync(Guid simulationId, AddSimulationEntryDto dto, CancellationToken ct = default)
    {
        var sourceType = (DailySimulationSourceType)dto.SourceType;

        var nutrients = sourceType == DailySimulationSourceType.Food
            ? await BuildFoodSnapshotAsync(dto.SourceId, dto.QuantityGrams, ct).ConfigureAwait(false)
            : await BuildDishSnapshotAsync(dto.SourceId, dto.QuantityGrams, ct).ConfigureAwait(false);

        if (nutrients is null)
        {
            return (null, sourceType == DailySimulationSourceType.Food
                ? "Alimento non trovato."
                : "Piatto non trovato.");
        }

        var entryId = Guid.NewGuid();
        var entry = new DailySimulationEntry
        {
            Id = entryId,
            SimulationId = simulationId,
            SectionId = dto.SectionId,
            SourceType = sourceType,
            SourceId = dto.SourceId,
            SnapshotAt = DateTime.UtcNow,
            QuantityGrams = dto.QuantityGrams
        };

        if (sourceType == DailySimulationSourceType.Food)
        {
            var food = await drRepository.GetFoodWithNutrientsAsync(dto.SourceId, ct).ConfigureAwait(false);
            entry.SourceName = food!.Name;
        }
        else
        {
            var dish = await drRepository.GetDishWithNutrientsAsync(dto.SourceId, ct).ConfigureAwait(false);
            entry.SourceName = dish!.Name;
        }

        foreach (var n in nutrients)
        {
            entry.Nutrients.Add(new DailySimulationEntryNutrient
            {
                EntryId = entryId,
                NutrientName = n.NutrientName,
                PositionOrder = n.PositionOrder,
                Quantity = n.Quantity,
                UnitAbbreviation = n.UnitAbbreviation
            });
        }

        var id = await drRepository.AddEntryAsync(entry, ct).ConfigureAwait(false);
        return (id, null);
    }

    public async Task<(bool Found, string? Error)> UpdateEntryQuantityAsync(Guid simulationId, Guid entryId, decimal newQuantityGrams, CancellationToken ct = default)
    {
        var entry = await drRepository.GetEntryAsync(entryId, ct).ConfigureAwait(false);

        if (entry is null || entry.SimulationId != simulationId)
            return (false, null);

        if (entry.SourceId is null)
            return (true, "Sorgente originale eliminata: impossibile ricalcolare.");

        var nutrients = entry.SourceType == DailySimulationSourceType.Food
            ? await BuildFoodSnapshotAsync(entry.SourceId.Value, newQuantityGrams, ct).ConfigureAwait(false)
            : await BuildDishSnapshotAsync(entry.SourceId.Value, newQuantityGrams, ct).ConfigureAwait(false);

        if (nutrients is null)
            return (true, "Sorgente originale eliminata: impossibile ricalcolare.");

        var newNutrients = nutrients.Select(n => new DailySimulationEntryNutrient
        {
            EntryId = entryId,
            NutrientName = n.NutrientName,
            PositionOrder = n.PositionOrder,
            Quantity = n.Quantity,
            UnitAbbreviation = n.UnitAbbreviation
        }).ToList();

        await drRepository.UpdateEntryNutrientsAsync(entryId, newQuantityGrams, newNutrients, ct).ConfigureAwait(false);
        return (true, null);
    }

    public async Task<bool> DeleteEntryAsync(Guid simulationId, Guid entryId, CancellationToken ct = default) =>
        await drRepository.DeleteEntryAsync(simulationId, entryId, ct).ConfigureAwait(false);

    public async Task<DailySimulationCompareDto?> CompareAsync(Guid id1, Guid id2, Guid userId, CancellationToken ct = default) =>
        await drRepository.CompareSimulationsAsync(id1, id2, userId, ct).ConfigureAwait(false);

    // ── Snapshot helpers ───────────────────────────────────

    private record NutrientSnapshot(string NutrientName, int PositionOrder, decimal Quantity, string UnitAbbreviation);

    private async Task<IList<NutrientSnapshot>?> BuildFoodSnapshotAsync(Guid foodId, decimal quantityGrams, CancellationToken ct)
    {
        var food = await drRepository.GetFoodWithNutrientsAsync(foodId, ct).ConfigureAwait(false);
        if (food is null) return null;

        var refQuantity = food.Quantity > 0 ? food.Quantity : 100m;

        return food.FoodsNutrients.Select(fn => new NutrientSnapshot(
            fn.Nutrient.Name,
            fn.Nutrient.PositionOrder,
            Math.Round(fn.Quantity * (quantityGrams / refQuantity), 4),
            fn.UnitOfMeasureNavigation.Abbreviation
        )).ToList();
    }

    private async Task<IList<NutrientSnapshot>?> BuildDishSnapshotAsync(Guid dishId, decimal quantityGrams, CancellationToken ct)
    {
        var dish = await drRepository.GetDishWithNutrientsAsync(dishId, ct).ConfigureAwait(false);
        if (dish is null) return null;

        var refWeight = dish.WeightGrams > 0 ? dish.WeightGrams : 100m;

        return dish.DishNutrients.Select(dn => new NutrientSnapshot(
            dn.Nutrient.Name,
            dn.Nutrient.PositionOrder,
            Math.Round(dn.Quantity * (quantityGrams / refWeight), 4),
            dn.UnitOfMeasureNavigation.Abbreviation
        )).ToList();
    }
}
