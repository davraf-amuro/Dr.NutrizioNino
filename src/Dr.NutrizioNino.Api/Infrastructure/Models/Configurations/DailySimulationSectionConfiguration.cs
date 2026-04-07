using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class DailySimulationSectionConfiguration : IEntityTypeConfiguration<DailySimulationSection>
{
    public void Configure(EntityTypeBuilder<DailySimulationSection> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK_DailySimulationSections");
        entity.ToTable("DailySimulationSections");

        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
        entity.Property(e => e.IsActive).HasDefaultValue(true);
    }
}
