namespace Dr.NutrizioNino.Models.Dto.Auth;

/// <summary>Scheda Sciaudone calcolata da una misurazione di profilo. Stima generica, non una prescrizione.</summary>
public record SciaudoneCardDto(
    Guid Id,
    Guid ProfileEntryId,
    DateTime ComputedAt,
    decimal WeightKg,
    decimal IdealWeightKg,
    decimal Kcal,
    decimal ProteinG,
    decimal FatG,
    decimal FiberG,
    decimal CarbsG
);
