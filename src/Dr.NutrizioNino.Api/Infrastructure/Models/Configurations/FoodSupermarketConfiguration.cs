using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations;

public class FoodSupermarketConfiguration : IEntityTypeConfiguration<FoodSupermarket>
{
    public void Configure(EntityTypeBuilder<FoodSupermarket> entity)
    {
        entity.ToTable("FoodSupermarket");
        entity.HasKey(e => new { e.FoodId, e.SupermarketId });

        entity.HasOne(e => e.Food)
            .WithMany(f => f.FoodSupermarkets)
            .HasForeignKey(e => e.FoodId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_FoodSupermarket_Foods");

        entity.HasOne(e => e.Supermarket)
            .WithMany(s => s.FoodSupermarkets)
            .HasForeignKey(e => e.SupermarketId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_FoodSupermarket_Supermarkets");
    }
}
