using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class DishIngredientConfiguration : IEntityTypeConfiguration<DishIngredient>
{
    public void Configure(EntityTypeBuilder<DishIngredient> entity)
    {
        entity.HasKey(e => new { e.DishId, e.FoodId }).HasName("PK_DishIngredients");
        entity.ToTable("DishIngredients");
        entity.Property(e => e.QuantityGrams).HasColumnType("numeric(6,2)");

        entity.HasOne(d => d.Dish)
            .WithMany(d => d.DishIngredients)
            .HasForeignKey(d => d.DishId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_DishIngredients_Dish");

        entity.HasOne(d => d.Food)
            .WithMany()
            .HasForeignKey(d => d.FoodId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_DishIngredients_Food");
    }
}
