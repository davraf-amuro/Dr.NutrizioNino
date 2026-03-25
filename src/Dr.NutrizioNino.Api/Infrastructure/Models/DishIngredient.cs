namespace Dr.NutrizioNino.Api.Models;

public class DishIngredient
{
    public Guid DishId { get; set; }
    public Guid FoodId { get; set; }
    public decimal QuantityGrams { get; set; }

    public virtual Food Dish { get; set; } = null!;
    public virtual Food Food { get; set; } = null!;
}
