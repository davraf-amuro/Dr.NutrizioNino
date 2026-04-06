using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class DailySimulationEntryConfiguration : IEntityTypeConfiguration<DailySimulationEntry>
{
    public void Configure(EntityTypeBuilder<DailySimulationEntry> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK_DailySimulationEntries");
        entity.ToTable("DailySimulationEntries");

        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.SectionType).HasConversion<byte>();
        entity.Property(e => e.SourceType).HasConversion<byte>();
        entity.Property(e => e.SourceName).IsRequired().HasMaxLength(200);
        entity.Property(e => e.QuantityGrams).HasColumnType("decimal(8,2)");
        entity.Property(e => e.SnapshotAt).HasColumnType("datetime2");

        entity.HasOne(d => d.Simulation)
            .WithMany(p => p.Entries)
            .HasForeignKey(d => d.SimulationId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DailySimulationEntries_Sim");

        entity.HasIndex(e => e.SimulationId).HasDatabaseName("IX_DailySimulationEntries_SimId");
    }
}
