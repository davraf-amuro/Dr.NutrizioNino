namespace Dr.NutrizioNino.Models.Dto;

// ── Richieste ──────────────────────────────────────────────

public record CreateDailySimulationDto(string Name);

public record RenameDailySimulationDto(string Name);

/// <summary>SectionType: 0=Colazione 1=Pranzo 2=Cena 3=Spuntino 4=Merenda 5=Altro</summary>
/// <summary>SourceType: 0=Food 1=Dish</summary>
public record AddSimulationEntryDto(byte SectionType, byte SourceType, Guid SourceId, decimal QuantityGrams);

public record UpdateEntryQuantityDto(decimal QuantityGrams);

// ── Risposte ───────────────────────────────────────────────

public record DailySimulationListItemDto(Guid Id, string Name, DateTime CreatedAt, int EntryCount);

public record DailySimulationEntryNutrientDto(
    string Name,
    int PositionOrder,
    decimal Quantity,
    string UnitAbbreviation
);

public record DailySimulationEntryDto(
    Guid Id,
    string SourceName,
    byte SourceType,
    decimal QuantityGrams,
    IList<DailySimulationEntryNutrientDto> Nutrients
);

public record DailySimulationSectionDto(
    byte SectionType,
    string SectionName,
    IList<DailySimulationEntryDto> Entries
);

public record DailySimulationDetailDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    IList<DailySimulationSectionDto> Sections
);

// ── Confronto ─────────────────────────────────────────────

public record SimulationCompareNutrientDto(
    string Name,
    int PositionOrder,
    decimal? Sim1Quantity,
    decimal? Sim2Quantity,
    string UnitAbbreviation
);

public record DailySimulationCompareDto(
    Guid Sim1Id,
    string Sim1Name,
    Guid Sim2Id,
    string Sim2Name,
    IList<SimulationCompareNutrientDto> Nutrients
);
