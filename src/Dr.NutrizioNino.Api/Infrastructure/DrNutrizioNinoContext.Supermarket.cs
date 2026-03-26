using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Models;

public partial class DrNutrizioNinoContext
{
    public virtual DbSet<Supermarket> Supermarkets { get; set; }
    public virtual DbSet<FoodSupermarket> FoodSupermarkets { get; set; }
}
