using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations;

public partial class FoodConfiguration
{
    partial void OnConfigurePartial(EntityTypeBuilder<Food> entity)
    {
        entity.Property(e => e.IsDish).HasDefaultValue(false);
    }
}
