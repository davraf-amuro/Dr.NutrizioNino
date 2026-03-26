namespace Dr.NutrizioNino.Api.Models;

public class DishNutrient
{
    public Guid DishId { get; set; }
    public Guid NutrientId { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public decimal Quantity { get; set; }

    public virtual Dish Dish { get; set; } = null!;
    public virtual Nutrient Nutrient { get; set; } = null!;
    public virtual UnitOfMeasure UnitOfMeasureNavigation { get; set; } = null!;
}
