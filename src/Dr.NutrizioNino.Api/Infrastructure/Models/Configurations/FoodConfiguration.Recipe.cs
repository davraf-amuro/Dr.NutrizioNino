using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations;

public partial class FoodConfiguration
{
    partial void OnConfigurePartial(EntityTypeBuilder<Food> entity)
    {
        // IsRecipe rimosso: le ricette sono ora nella tabella separata Recipes

        entity.HasOne(e => e.Owner)
              .WithMany()
              .HasForeignKey(e => e.OwnerId)
              .OnDelete(DeleteBehavior.SetNull)
              .HasConstraintName("FK_Foods_Owner");
    }
}
