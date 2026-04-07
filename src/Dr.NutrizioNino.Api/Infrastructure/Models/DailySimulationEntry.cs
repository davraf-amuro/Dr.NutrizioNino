namespace Dr.NutrizioNino.Api.Models;

public enum DailySimulationSourceType : byte
{
    Food = 0,
    Dish = 1
}

public class DailySimulationEntry
{
    public Guid Id { get; set; }
    public Guid SimulationId { get; set; }
    public Guid SectionId { get; set; }
    public DailySimulationSourceType SourceType { get; set; }
    public Guid? SourceId { get; set; }
    public string SourceName { get; set; } = null!;
    public decimal QuantityGrams { get; set; }
    public DateTime SnapshotAt { get; set; }

    public virtual DailySimulation Simulation { get; set; } = null!;
    public virtual DailySimulationSection Section { get; set; } = null!;
    public virtual ICollection<DailySimulationEntryNutrient> Nutrients { get; set; } = [];
}
