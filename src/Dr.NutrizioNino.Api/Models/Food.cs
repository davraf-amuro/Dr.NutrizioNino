
namespace Dr.NutrizioNino.Api.Models;

public partial class Food
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Guid UnitOfMeasureId { get; set; }

    public decimal Quantity { get; set; }

    public string Barcode { get; set; }

    public Guid? BrandId { get; set; }

    public int Calorie { get; set; }

    public virtual Brand Brand { get; set; }

    public virtual UnitOfMeasure UnitOfMeasure { get; set; }

    public virtual ICollection<Nutrient> Nutrients { get; set; } = new List<Nutrient>();
}