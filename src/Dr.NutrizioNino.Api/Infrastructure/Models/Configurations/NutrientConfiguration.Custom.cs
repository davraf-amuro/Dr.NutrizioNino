using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations;

public partial class NutrientConfiguration
{
    /// <summary>Configura la relazione Nutrient → UnitOfMeasure così che le proiezioni possano leggere l'abbreviazione via JOIN.</summary>
    partial void OnConfigurePartial(EntityTypeBuilder<Nutrient> entity)
    {
        // FK obbligatoria verso l'unità di misura canonica; no cascade per non eliminare nutrienti se si rimuove una UoM
        entity.HasOne(e => e.DefaultUnitOfMeasure)
            .WithMany()
            .HasForeignKey(e => e.DefaultUnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
