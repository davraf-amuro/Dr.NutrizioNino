namespace Dr.NutrizioNino.Api.Models;

public class Supermarket
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<FoodSupermarket> FoodSupermarkets { get; set; } = [];
}
