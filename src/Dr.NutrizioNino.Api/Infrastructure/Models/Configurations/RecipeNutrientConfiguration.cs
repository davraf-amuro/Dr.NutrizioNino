using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class RecipeNutrientConfiguration : IEntityTypeConfiguration<RecipeNutrient>
{
    public void Configure(EntityTypeBuilder<RecipeNutrient> entity)
    {
        entity.HasKey(e => new { e.RecipeId, e.NutrientId }).HasName("PK_Recipes_Nutrients");
        entity.ToTable("Recipes_Nutrients");

        entity.Property(e => e.Quantity).HasColumnType("numeric(6,2)");

        entity.HasOne(d => d.Recipe)
            .WithMany(p => p.RecipeNutrients)
            .HasForeignKey(d => d.RecipeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Recipes_Nutrients_Recipes");

        entity.HasOne(d => d.Nutrient)
            .WithMany()
            .HasForeignKey(d => d.NutrientId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Recipes_Nutrients_Nutrients");

        entity.HasOne(d => d.UnitOfMeasureNavigation)
            .WithMany()
            .HasForeignKey(d => d.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Recipes_Nutrients_UnitsOfMeasures");
    }
}
