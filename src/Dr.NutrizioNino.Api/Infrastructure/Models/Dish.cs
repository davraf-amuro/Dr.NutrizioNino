namespace Dr.NutrizioNino.Api.Models;

public class Dish
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal Calorie { get; set; }
    public Guid UnitOfMeasureId { get; set; }

    public virtual UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    public virtual ICollection<DishNutrient> DishNutrients { get; set; } = [];
    public virtual ICollection<DishIngredient> DishIngredients { get; set; } = [];
}
