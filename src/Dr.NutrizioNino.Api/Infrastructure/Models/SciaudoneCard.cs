namespace Dr.NutrizioNino.Api.Models;

/// <summary>Scheda Sciaudone calcolata da una singola misurazione di profilo (storicizzata).</summary>
public class SciaudoneCard
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>Misurazione di profilo da cui la scheda è stata calcolata.</summary>
    public Guid ProfileEntryId { get; set; }

    /// <summary>Peso attuale usato nel calcolo, copiato dalla misurazione.</summary>
    public decimal WeightKg { get; set; }

    /// <summary>Peso ideale dichiarato dall'utente, copiato dalla misurazione.</summary>
    public decimal IdealWeightKg { get; set; }

    /// <summary>Calorie giornaliere per il dimagrimento.</summary>
    public decimal Kcal { get; set; }

    /// <summary>Proteine giornaliere in grammi.</summary>
    public decimal ProteinG { get; set; }

    /// <summary>Grassi giornalieri in grammi.</summary>
    public decimal FatG { get; set; }

    /// <summary>Fibre giornaliere in grammi.</summary>
    public decimal FiberG { get; set; }

    /// <summary>Carboidrati giornalieri in grammi.</summary>
    public decimal CarbsG { get; set; }

    public DateTime ComputedAt { get; set; }

    public UserProfileEntry ProfileEntry { get; set; } = null!;
}
