namespace Dr.NutrizioNino.Api.Models;

public partial class Nutrient
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public int UnitOfMeasure { get; set; }

    public decimal Quantity { get; set; }

    public virtual ICollection<Food> Foods { get; set; } = new List<Food>();

}