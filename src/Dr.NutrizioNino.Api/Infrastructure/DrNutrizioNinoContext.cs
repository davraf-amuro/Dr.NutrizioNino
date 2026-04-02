using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Models.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Models;

public partial class DrNutrizioNinoContext(
    DbContextOptions<DrNutrizioNinoContext> options,
    ILoggerFactory loggerFactory) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Food> Foods { get; set; }

    public virtual DbSet<FoodNutrient> FoodsNutrients { get; set; }

    public virtual DbSet<Nutrient> Nutrients { get; set; }

    public virtual DbSet<UnitOfMeasure> UnitsOfMeasures { get; set; }

    public virtual DbSet<NutrientsGetForFoodCreatingInfo> NutrientsGetForFoodCreatingInfoes { get; set; }

    public virtual DbSet<FoodDashboardInfo> FoodsDashboard { get; set; }

    public virtual DbSet<NutrientInfo> NutrientInfoes { get; set; }

    public virtual DbSet<UserProfileEntry> UserProfileEntries { get; set; }

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
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new BrandConfiguration());
        modelBuilder.ApplyConfiguration(new FoodConfiguration());
        modelBuilder.ApplyConfiguration(new FoodsNutrientConfiguration());
        modelBuilder.ApplyConfiguration(new NutrientConfiguration());
        modelBuilder.ApplyConfiguration(new UnitsOfMeasureConfiguration());
        modelBuilder.ApplyConfiguration(new FoodDashboardConfiguration());
        modelBuilder.ApplyConfiguration(new UserProfileEntryConfiguration());

        modelBuilder.Entity<ApplicationUser>()
            .HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("IX_AspNetUsers_NormalizedEmail_Unique");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
