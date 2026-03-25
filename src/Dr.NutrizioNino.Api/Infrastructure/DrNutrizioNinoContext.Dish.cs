using Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Models;

public partial class DrNutrizioNinoContext
{
    public virtual DbSet<DishIngredient> DishIngredients { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DishIngredientConfiguration());
    }
}
