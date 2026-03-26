using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations;

public class SupermarketConfiguration : IEntityTypeConfiguration<Supermarket>
{
    public void Configure(EntityTypeBuilder<Supermarket> entity)
    {
        entity.ToTable("Supermarkets");
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).ValueGeneratedNever();
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);
        entity.HasIndex(e => e.Name).IsUnique();
    }
}
