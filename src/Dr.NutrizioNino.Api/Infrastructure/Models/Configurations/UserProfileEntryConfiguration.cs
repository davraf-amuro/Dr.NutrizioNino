using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;

public class UserProfileEntryConfiguration : IEntityTypeConfiguration<UserProfileEntry>
{
    public void Configure(EntityTypeBuilder<UserProfileEntry> entity)
    {
        entity.HasKey(e => e.Id);
        entity.ToTable("UserProfileEntries");
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.RecordedAt).IsRequired();
        entity.Property(e => e.WeightKg).HasColumnType("numeric(5,2)");
        entity.Property(e => e.HeightCm).HasColumnType("numeric(5,2)");
        entity.Property(e => e.Sex).HasMaxLength(1);
        entity.Property(e => e.Job).HasMaxLength(20);

        entity.HasOne(e => e.User)
              .WithMany(u => u.ProfileEntries)
              .HasForeignKey(e => e.UserId)
              .OnDelete(DeleteBehavior.Cascade)
              .HasConstraintName("FK_UserProfileEntries_AspNetUsers");
    }
}
