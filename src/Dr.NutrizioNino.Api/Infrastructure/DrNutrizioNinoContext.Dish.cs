using Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;
using Dr.NutrizioNino.Api.Models.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Models;

public partial class DrNutrizioNinoContext
{
    public virtual DbSet<Dish> Dishes { get; set; }
    public virtual DbSet<DishNutrient> DishNutrients { get; set; }
    public virtual DbSet<DishIngredient> DishIngredients { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new DishConfiguration());
        modelBuilder.ApplyConfiguration(new DishNutrientConfiguration());
        modelBuilder.ApplyConfiguration(new DishIngredientConfiguration());
        modelBuilder.ApplyConfiguration(new SupermarketConfiguration());
        modelBuilder.ApplyConfiguration(new FoodSupermarketConfiguration());
    }
}
