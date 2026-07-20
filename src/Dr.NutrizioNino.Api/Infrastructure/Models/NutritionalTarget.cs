namespace Dr.NutrizioNino.Api.Models;

public class NutritionalTarget
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal? KcalTarget { get; set; }
    public decimal? CarbsTarget { get; set; }
    public decimal? ProteinTarget { get; set; }
    public decimal? FatTarget { get; set; }
    public DateTime UpdatedAt { get; set; }
}
