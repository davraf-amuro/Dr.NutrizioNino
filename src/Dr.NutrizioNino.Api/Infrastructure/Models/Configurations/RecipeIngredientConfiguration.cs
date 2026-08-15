using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> entity)
    {
        entity.HasKey(e => new { e.RecipeId, e.FoodId }).HasName("PK_RecipeIngredients");
        entity.ToTable("RecipeIngredients");
        entity.Property(e => e.QuantityGrams).HasColumnType("numeric(6,2)");

        entity.HasOne(d => d.Recipe)
            .WithMany(d => d.RecipeIngredients)
            .HasForeignKey(d => d.RecipeId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_RecipeIngredients_Recipe");

        entity.HasOne(d => d.Food)
            .WithMany()
            .HasForeignKey(d => d.FoodId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_RecipeIngredients_Food");
    }
}
