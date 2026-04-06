namespace Dr.NutrizioNino.Api.Models;

public class DailySimulationEntryNutrient
{
    public Guid EntryId { get; set; }
    public string NutrientName { get; set; } = null!;
    public int PositionOrder { get; set; }
    public decimal Quantity { get; set; }
    public string UnitAbbreviation { get; set; } = null!;

    public virtual DailySimulationEntry Entry { get; set; } = null!;
}
