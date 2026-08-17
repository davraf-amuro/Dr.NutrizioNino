namespace Dr.NutrizioNino.Api.Models;

public class UserProfileEntry
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime RecordedAt { get; set; }
    public decimal? WeightKg { get; set; }
    /// <summary>Peso ideale dichiarato dall'utente: base di calcolo delle proteine della scheda Sciaudone.</summary>
    public decimal? IdealWeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    /// <summary>"M" o "F"</summary>
    public string? Sex { get; set; }
    /// <summary>sedentario | moderato | attivo | molto_attivo</summary>
    public string? Job { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
