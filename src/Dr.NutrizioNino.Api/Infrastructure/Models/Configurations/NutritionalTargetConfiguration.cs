using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class NutritionalTargetConfiguration : IEntityTypeConfiguration<NutritionalTarget>
{
    public void Configure(EntityTypeBuilder<NutritionalTarget> entity)
    {
        entity.HasKey(e => e.Id).HasName("PK_NutritionalTargets");
        entity.ToTable("NutritionalTargets");

        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.KcalTarget).HasColumnType("decimal(6,1)");
        entity.Property(e => e.CarbsTarget).HasColumnType("decimal(6,1)");
        entity.Property(e => e.ProteinTarget).HasColumnType("decimal(6,1)");
        entity.Property(e => e.FatTarget).HasColumnType("decimal(6,1)");
        entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");

        entity.HasOne<ApplicationUser>()
              .WithMany()
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade)
              .HasConstraintName("FK_NutritionalTargets_User");

        entity.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("UQ_NutritionalTargets_UserId");
    }
}
