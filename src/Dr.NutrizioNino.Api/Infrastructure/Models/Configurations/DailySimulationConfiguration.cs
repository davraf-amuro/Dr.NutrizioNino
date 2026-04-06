using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class DailySimulationConfiguration : IEntityTypeConfiguration<DailySimulation>
{
    public void Configure(EntityTypeBuilder<DailySimulation> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK_DailySimulations");
        entity.ToTable("DailySimulations");

        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.Property(e => e.CreatedAt).HasColumnType("datetime2");
        entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");

        entity.HasOne(d => d.User)
            .WithMany()
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_DailySimulations_User");

        entity.HasIndex(e => e.UserId).HasDatabaseName("IX_DailySimulations_UserId");
    }
}
