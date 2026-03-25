namespace Dr.NutrizioNino.Api.Models;

public partial class Food
{
    public bool IsDish { get; set; }
    public virtual ICollection<DishIngredient> DishIngredients { get; set; } = new List<DishIngredient>();
}
