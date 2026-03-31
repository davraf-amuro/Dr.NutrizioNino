using Dr.NutrizioNino.Api.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class DishDashboardConfiguration : IEntityTypeConfiguration<DishDashboardInfo>
{
    public void Configure(EntityTypeBuilder<DishDashboardInfo> entity)
    {
        entity.ToView("Dishes_Dashboard");
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.HasKey(e => e.Id);
    }
}
