using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Infrastructure;

public partial class DrRepository
{
    public async Task<IList<DailySimulationListItemDto>> GetUserSimulationsAsync(Guid userId, CancellationToken ct = default) =>
        await drContext.DailySimulations
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new DailySimulationListItemDto(
                s.Id,
                s.Name,
                s.CreatedAt,
                s.Entries.Count))
            .ToListAsync(ct)
            .ConfigureAwait(false);

    public async Task<DailySimulationDetailDto?> GetSimulationDetailAsync(Guid id, CancellationToken ct = default)
    {
        var sim = await drContext.DailySimulations
            .AsNoTracking()
            .Include(s => s.Entries)
                .ThenInclude(e => e.Nutrients)
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            .ConfigureAwait(false);

        if (sim is null) return null;

        var sections = sim.Entries
            .GroupBy(e => e.SectionType)
            .OrderBy(g => g.Key)
            .Select(g => new DailySimulationSectionDto(
                (byte)g.Key,
                g.Key.ToDisplayName(),
                g.OrderBy(e => e.SnapshotAt)
                 .Select(e => new DailySimulationEntryDto(
                     e.Id,
                     e.SourceName,
                     (byte)e.SourceType,
                     e.QuantityGrams,
                     e.Nutrients
                      .OrderBy(n => n.PositionOrder)
                      .Select(n => new DailySimulationEntryNutrientDto(n.NutrientName, n.PositionOrder, n.Quantity, n.UnitAbbreviation))
                      .ToList()))
                 .ToList()))
            .ToList();

        return new DailySimulationDetailDto(sim.Id, sim.Name, sim.CreatedAt, sections);
    }

    public async Task<Guid?> GetSimulationOwnerIdAsync(Guid id, CancellationToken ct = default) =>
        await drContext.DailySimulations
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => (Guid?)s.UserId)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task<Guid> CreateSimulationAsync(DailySimulation simulation, CancellationToken ct = default)
    {
        drContext.DailySimulations.Add(simulation);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return simulation.Id;
    }

    public async Task<bool> RenameSimulationAsync(Guid id, string name, CancellationToken ct = default)
    {
        var record = await drContext.DailySimulations
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            .ConfigureAwait(false);

        if (record is null) return false;

        record.Name = name;
        record.UpdatedAt = DateTime.UtcNow;
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task DeleteSimulationAsync(Guid id, CancellationToken ct = default)
    {
        var record = await drContext.DailySimulations
            .FirstOrDefaultAsync(s => s.Id == id, ct)
            .ConfigureAwait(false);

        if (record is null) return;

        drContext.DailySimulations.Remove(record);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
    }

    public async Task<Guid> AddEntryAsync(DailySimulationEntry entry, CancellationToken ct = default)
    {
        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.DailySimulationEntries.Add(entry);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return entry.Id;
    }

    public async Task<bool> UpdateEntryNutrientsAsync(Guid entryId, decimal newQuantityGrams, IList<DailySimulationEntryNutrient> newNutrients, CancellationToken ct = default)
    {
        var entry = await drContext.DailySimulationEntries
            .Include(e => e.Nutrients)
            .FirstOrDefaultAsync(e => e.Id == entryId, ct)
            .ConfigureAwait(false);

        if (entry is null) return false;

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        entry.QuantityGrams = newQuantityGrams;
        entry.SnapshotAt = DateTime.UtcNow;
        drContext.DailySimulationEntryNutrients.RemoveRange(entry.Nutrients);
        drContext.DailySimulationEntryNutrients.AddRange(newNutrients);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> DeleteEntryAsync(Guid simulationId, Guid entryId, CancellationToken ct = default)
    {
        var entry = await drContext.DailySimulationEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.SimulationId == simulationId, ct)
            .ConfigureAwait(false);

        if (entry is null) return false;

        drContext.DailySimulationEntries.Remove(entry);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        return true;
    }

    public async Task<DailySimulationEntry?> GetEntryAsync(Guid entryId, CancellationToken ct = default) =>
        await drContext.DailySimulationEntries
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == entryId, ct)
            .ConfigureAwait(false);

    public async Task<Guid> CloneSimulationAsync(Guid sourceId, Guid userId, string newName, CancellationToken ct = default)
    {
        var source = await drContext.DailySimulations
            .AsNoTracking()
            .Include(s => s.Entries)
                .ThenInclude(e => e.Nutrients)
            .FirstOrDefaultAsync(s => s.Id == sourceId, ct)
            .ConfigureAwait(false);

        if (source is null) throw new InvalidOperationException("Simulazione sorgente non trovata.");

        var cloneId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var clone = new DailySimulation
        {
            Id = cloneId,
            UserId = userId,
            Name = newName,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var sourceEntry in source.Entries)
        {
            var cloneEntryId = Guid.NewGuid();
            var cloneEntry = new DailySimulationEntry
            {
                Id = cloneEntryId,
                SimulationId = cloneId,
                SectionType = sourceEntry.SectionType,
                SourceType = sourceEntry.SourceType,
                SourceId = sourceEntry.SourceId,
                SourceName = sourceEntry.SourceName,
                QuantityGrams = sourceEntry.QuantityGrams,
                SnapshotAt = now
            };

            foreach (var n in sourceEntry.Nutrients)
            {
                cloneEntry.Nutrients.Add(new DailySimulationEntryNutrient
                {
                    EntryId = cloneEntryId,
                    NutrientName = n.NutrientName,
                    PositionOrder = n.PositionOrder,
                    Quantity = n.Quantity,
                    UnitAbbreviation = n.UnitAbbreviation
                });
            }

            clone.Entries.Add(cloneEntry);
        }

        await using var transaction = await drContext.Database.BeginTransactionAsync(ct).ConfigureAwait(false);
        drContext.DailySimulations.Add(clone);
        await drContext.SaveChangesAsync(ct).ConfigureAwait(false);
        await transaction.CommitAsync(ct).ConfigureAwait(false);
        return cloneId;
    }

    public async Task<DailySimulationCompareDto?> CompareSimulationsAsync(Guid id1, Guid id2, Guid userId, CancellationToken ct = default)
    {
        var sims = await drContext.DailySimulations
            .AsNoTracking()
            .Where(s => (s.Id == id1 || s.Id == id2) && s.UserId == userId)
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (sims.Count < 2) return null;

        var sim1Info = sims.First(s => s.Id == id1);
        var sim2Info = sims.First(s => s.Id == id2);

        var nutrientsBySim = await drContext.DailySimulationEntryNutrients
            .AsNoTracking()
            .Where(n => drContext.DailySimulationEntries
                .Any(e => e.Id == n.EntryId &&
                          (e.SimulationId == id1 || e.SimulationId == id2)))
            .Join(drContext.DailySimulationEntries,
                n => n.EntryId,
                e => e.Id,
                (n, e) => new { e.SimulationId, n.NutrientName, n.PositionOrder, n.Quantity, n.UnitAbbreviation })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var allNutrientNames = nutrientsBySim.Select(x => x.NutrientName).Distinct();

        var nutrients = allNutrientNames
            .Select(name =>
            {
                var group = nutrientsBySim.Where(x => x.NutrientName == name).ToList();
                var posOrder = group.First().PositionOrder;
                var unit = group.First().UnitAbbreviation;
                var sim1Total = group.Where(x => x.SimulationId == id1).Sum(x => x.Quantity);
                var sim2Total = group.Where(x => x.SimulationId == id2).Sum(x => x.Quantity);
                return new SimulationCompareNutrientDto(
                    name, posOrder,
                    nutrientsBySim.Any(x => x.SimulationId == id1 && x.NutrientName == name) ? sim1Total : null,
                    nutrientsBySim.Any(x => x.SimulationId == id2 && x.NutrientName == name) ? sim2Total : null,
                    unit);
            })
            .OrderBy(n => n.PositionOrder == 0 ? int.MaxValue : n.PositionOrder)
            .ThenBy(n => n.Name)
            .ToList();

        return new DailySimulationCompareDto(id1, sim1Info.Name, id2, sim2Info.Name, nutrients);
    }

    // ── Query helper per il service ──────────────────────────

    internal async Task<Food?> GetFoodWithNutrientsAsync(Guid foodId, CancellationToken ct = default) =>
        await drContext.Foods
            .AsNoTracking()
            .Include(f => f.FoodsNutrients)
                .ThenInclude(fn => fn.Nutrient)
            .Include(f => f.FoodsNutrients)
                .ThenInclude(fn => fn.UnitOfMeasureNavigation)
            .FirstOrDefaultAsync(f => f.Id == foodId, ct)
            .ConfigureAwait(false);

    internal async Task<Dish?> GetDishWithNutrientsAsync(Guid dishId, CancellationToken ct = default) =>
        await drContext.Dishes
            .AsNoTracking()
            .Include(d => d.DishNutrients)
                .ThenInclude(dn => dn.Nutrient)
            .Include(d => d.DishNutrients)
                .ThenInclude(dn => dn.UnitOfMeasureNavigation)
            .FirstOrDefaultAsync(d => d.Id == dishId, ct)
            .ConfigureAwait(false);
}
