using Dr.NutrizioNino.Api.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class RecipeDashboardConfiguration : IEntityTypeConfiguration<RecipeDashboardInfo>
{
    public void Configure(EntityTypeBuilder<RecipeDashboardInfo> entity)
    {
        entity.ToView("Recipes_Dashboard");
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.HasKey(e => e.Id);
    }
}
