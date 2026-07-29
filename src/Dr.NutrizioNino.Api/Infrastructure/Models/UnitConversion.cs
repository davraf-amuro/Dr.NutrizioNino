namespace Dr.NutrizioNino.Api.Models;

public partial class UnitConversion
{
    public Guid Id { get; set; }
    public string FromUnit { get; set; } = null!;
    public string ToUnit { get; set; } = null!;
    public decimal Factor { get; set; }
}
