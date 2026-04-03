using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class FoodCategoryConfiguration : IEntityTypeConfiguration<FoodCategory>
{
    public void Configure(EntityTypeBuilder<FoodCategory> entity)
    {
        entity.ToTable("FoodCategory");
        entity.HasKey(e => new { e.FoodId, e.CategoryId });

        entity.HasOne(e => e.Food)
            .WithMany(f => f.FoodCategories)
            .HasForeignKey(e => e.FoodId)
            .HasConstraintName("FK_FoodCategory_Foods");

        entity.HasOne(e => e.Category)
            .WithMany(c => c.FoodCategories)
            .HasForeignKey(e => e.CategoryId)
            .HasConstraintName("FK_FoodCategory_Categories");
    }
}
