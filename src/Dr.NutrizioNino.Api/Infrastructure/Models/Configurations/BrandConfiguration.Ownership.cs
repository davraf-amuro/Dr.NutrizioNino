using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations;

public partial class BrandConfiguration
{
    partial void OnConfigurePartial(EntityTypeBuilder<Brand> entity)
    {
        entity.HasOne(e => e.Owner)
              .WithMany()
              .HasForeignKey(e => e.OwnerId)
              .OnDelete(DeleteBehavior.SetNull)
              .HasConstraintName("FK_Brands_Owner");
    }
}
