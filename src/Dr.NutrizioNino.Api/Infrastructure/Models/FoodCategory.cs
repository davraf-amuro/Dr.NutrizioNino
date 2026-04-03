namespace Dr.NutrizioNino.Api.Models;

public class FoodCategory
{
    public Guid FoodId { get; set; }

    public Guid CategoryId { get; set; }

    public virtual Food Food { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;
}
