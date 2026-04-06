namespace Dr.NutrizioNino.Api.Models;

public class DailySimulation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual ApplicationUser User { get; set; } = null!;
    public virtual ICollection<DailySimulationEntry> Entries { get; set; } = [];
}
