using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class DailySimulationEntryNutrientConfiguration : IEntityTypeConfiguration<DailySimulationEntryNutrient>
{
    public void Configure(EntityTypeBuilder<DailySimulationEntryNutrient> entity)
    {
        entity.HasKey(e => new { e.EntryId, e.NutrientName }).HasName("PK_DSEntryNutrients");
        entity.ToTable("DailySimulationEntryNutrients");

        entity.Property(e => e.NutrientName).IsRequired().HasMaxLength(100);
        entity.Property(e => e.Quantity).HasColumnType("decimal(10,4)");
        entity.Property(e => e.UnitAbbreviation).IsRequired().HasMaxLength(20);

        entity.HasOne(d => d.Entry)
            .WithMany(p => p.Nutrients)
            .HasForeignKey(d => d.EntryId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DSEntryNutrients_Entry");
    }
}
