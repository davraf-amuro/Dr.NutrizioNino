using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;
using Dr.NutrizioNino.Api.Models.Configurations;
using Microsoft.EntityFrameworkCore;


namespace Dr.NutrizioNino.Api.Models;

public partial class DrNutrizioNinoContext
{
    public virtual DbSet<Recipe>? Recipes { get; set; }
    public virtual DbSet<RecipeNutrient>? RecipeNutrients { get; set; }
    public virtual DbSet<RecipeIngredient>? RecipeIngredients { get; set; }
    public virtual DbSet<RecipeDashboardInfo>? RecipesDashboard { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new RecipeConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeNutrientConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeIngredientConfiguration());
        modelBuilder.ApplyConfiguration(new SupermarketConfiguration());
        modelBuilder.ApplyConfiguration(new FoodSupermarketConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeDashboardConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());
        modelBuilder.ApplyConfiguration(new FoodCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new DailySimulationConfiguration());
        modelBuilder.ApplyConfiguration(new DailySimulationEntryConfiguration());
        modelBuilder.ApplyConfiguration(new DailySimulationEntryNutrientConfiguration());
        modelBuilder.ApplyConfiguration(new UnitConversionConfiguration());
    }
}
