using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class SciaudoneCardConfiguration : IEntityTypeConfiguration<SciaudoneCard>
{
    public void Configure(EntityTypeBuilder<SciaudoneCard> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK_SciaudoneCards");
        entity.ToTable("SciaudoneCards");

        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.WeightKg).HasColumnType("numeric(5,2)");
        entity.Property(e => e.IdealWeightKg).HasColumnType("numeric(5,2)");
        entity.Property(e => e.Kcal).HasColumnType("decimal(6,1)");
        entity.Property(e => e.ProteinG).HasColumnType("decimal(6,1)");
        entity.Property(e => e.FatG).HasColumnType("decimal(6,1)");
        entity.Property(e => e.FiberG).HasColumnType("decimal(6,1)");
        entity.Property(e => e.CarbsG).HasColumnType("decimal(6,1)");
        entity.Property(e => e.ComputedAt).HasColumnType("datetime2");

        // NoAction: con Cascade anche qui si creerebbe un secondo percorso di
        // cancellazione verso AspNetUsers (SQL Server error 1785)
        entity.HasOne<ApplicationUser>()
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.NoAction)
              .HasConstraintName("FK_SciaudoneCards_User");

        entity.HasOne(e => e.ProfileEntry)
              .WithMany()
              .HasForeignKey(e => e.ProfileEntryId)
              .OnDelete(DeleteBehavior.Cascade)
              .HasConstraintName("FK_SciaudoneCards_ProfileEntry");

        entity.HasIndex(e => e.ProfileEntryId).IsUnique().HasDatabaseName("UQ_SciaudoneCards_ProfileEntryId");
        entity.HasIndex(e => new { e.UserId, e.ComputedAt }).HasDatabaseName("IX_SciaudoneCards_UserId_ComputedAt");
    }
}
