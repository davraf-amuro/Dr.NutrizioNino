namespace Dr.NutrizioNino.Api.Models;

public class RecipeIngredient
{
    public Guid RecipeId { get; set; }
    public Guid FoodId { get; set; }
    public decimal QuantityGrams { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;
    public virtual Food Food { get; set; } = null!;
}
