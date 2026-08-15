namespace Dr.NutrizioNino.Api.Models;

public class RecipeNutrient
{
    public Guid RecipeId { get; set; }
    public Guid NutrientId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal Quantity { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;
    public virtual Nutrient Nutrient { get; set; } = null!;
    public virtual UnitOfMeasure UnitOfMeasureNavigation { get; set; } = null!;
}
