using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class DishNutrientConfiguration : IEntityTypeConfiguration<DishNutrient>
{
    public void Configure(EntityTypeBuilder<DishNutrient> entity)
    {
        entity.HasKey(e => new { e.DishId, e.NutrientId }).HasName("PK_Dishes_Nutrients");
        entity.ToTable("Dishes_Nutrients");

        entity.Property(e => e.Quantity).HasColumnType("numeric(6,2)");

        entity.HasOne(d => d.Dish)
            .WithMany(p => p.DishNutrients)
            .HasForeignKey(d => d.DishId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Dishes_Nutrients_Dishes");

        entity.HasOne(d => d.Nutrient)
            .WithMany()
            .HasForeignKey(d => d.NutrientId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Dishes_Nutrients_Nutrients");

        entity.HasOne(d => d.UnitOfMeasureNavigation)
            .WithMany()
            .HasForeignKey(d => d.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Dishes_Nutrients_UnitsOfMeasures");
    }
}
