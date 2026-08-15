using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK_Recipes");
        entity.ToTable("Recipes");

        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
        entity.Property(e => e.WeightGrams).HasColumnName("WeightGrams").HasColumnType("numeric(6,2)");
        entity.Property(e => e.IsNutritionStale).IsRequired().HasDefaultValue(false);
        entity.Property(e => e.NutrientsCalculatedAt).HasColumnType("datetime");

        entity.HasOne(d => d.UnitOfMeasure)
            .WithMany()
            .HasForeignKey(d => d.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_Recipes_UnitsOfMeasures");

        entity.HasOne(d => d.Owner)
            .WithMany()
            .HasForeignKey(d => d.OwnerId)
            .OnDelete(DeleteBehavior.SetNull)
            .HasConstraintName("FK_Recipes_Owner");
    }
}
