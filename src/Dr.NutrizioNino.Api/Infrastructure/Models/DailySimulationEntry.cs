namespace Dr.NutrizioNino.Api.Models;

public enum DailySimulationSectionType : byte
{
    Colazione = 0,
    Pranzo = 1,
    Cena = 2,
    Spuntino = 3,
    Merenda = 4,
    Altro = 5
}

public enum DailySimulationSourceType : byte
{
    Food = 0,
    Dish = 1
}

public class DailySimulationEntry
{
    public Guid Id { get; set; }
    public Guid SimulationId { get; set; }
    public DailySimulationSectionType SectionType { get; set; }
    public DailySimulationSourceType SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string SourceName { get; set; } = null!;
    public decimal QuantityGrams { get; set; }
    public DateTime SnapshotAt { get; set; }

    public virtual DailySimulation Simulation { get; set; } = null!;
    public virtual ICollection<DailySimulationEntryNutrient> Nutrients { get; set; } = [];
}
