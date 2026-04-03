namespace Dr.NutrizioNino.Api.Models;

public class Category
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<FoodCategory> FoodCategories { get; set; } = [];
}
