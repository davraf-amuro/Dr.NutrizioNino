using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations;

public class UnitConversionConfiguration : IEntityTypeConfiguration<UnitConversion>
{
    public void Configure(EntityTypeBuilder<UnitConversion> entity)
    {
        entity.ToTable("UnitConversions");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.FromUnit).IsRequired().HasMaxLength(10);
        entity.Property(e => e.ToUnit).IsRequired().HasMaxLength(10);
        entity.Property(e => e.Factor).HasPrecision(20, 10);
        entity.HasIndex(e => new { e.FromUnit, e.ToUnit }).IsUnique();
    }
}
