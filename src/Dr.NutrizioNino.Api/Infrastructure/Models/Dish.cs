namespace Dr.NutrizioNino.Api.Models;

public class Dish
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public decimal WeightGrams { get; set; }
    public decimal Calorie { get; set; }
    public Guid UnitOfMeasureId { get; set; }
    public bool IsNutritionStale { get; set; }
    public DateTime? NutrientsCalculatedAt { get; set; }

    public Guid? OwnerId { get; set; }

    public virtual ApplicationUser? Owner { get; set; }
    public virtual UnitOfMeasure UnitOfMeasure { get; set; } = null!;
    public virtual ICollection<DishNutrient> DishNutrients { get; set; } = [];
    public virtual ICollection<DishIngredient> DishIngredients { get; set; } = [];
}
