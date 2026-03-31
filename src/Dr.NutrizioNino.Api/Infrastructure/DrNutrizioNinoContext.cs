using Dr.NutrizioNino.Api.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Models;

public partial class DrNutrizioNinoContext(
    DbContextOptions<DrNutrizioNinoContext> options,
    ILoggerFactory loggerFactory) : DbContext(options)
{
    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Food> Foods { get; set; }

    public virtual DbSet<FoodNutrient> FoodsNutrients { get; set; }

    public virtual DbSet<Nutrient> Nutrients { get; set; }

    public virtual DbSet<UnitOfMeasure> UnitsOfMeasures { get; set; }

    public virtual DbSet<NutrientsGetForFoodCreatingInfo> NutrientsGetForFoodCreatingInfoes { get; set; }

    public virtual DbSet<FoodDashboardInfo> FoodsDashboard { get; set; }

    public virtual DbSet<NutrientInfo> NutrientInfoes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && loggerFactory != null)
        {
            optionsBuilder.UseLoggerFactory(loggerFactory);
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.BrandConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.FoodConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.FoodsNutrientConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NutrientConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.UnitsOfMeasureConfiguration());
        //modelBuilder.ApplyConfiguration(new Configurations.NutrientsGetForFoodCreatingInfoConfiguration());
        modelBuilder.ApplyConfiguration(new Dr.NutrizioNino.Api.Infrastructure.Models.Configurations.FoodDashboardConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
