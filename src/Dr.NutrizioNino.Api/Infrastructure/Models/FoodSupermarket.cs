namespace Dr.NutrizioNino.Api.Models;

public class FoodSupermarket
{
    public Guid FoodId { get; set; }

    public Guid SupermarketId { get; set; }

    public virtual Food Food { get; set; } = null!;

    public virtual Supermarket Supermarket { get; set; } = null!;
}
