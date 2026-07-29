namespace Dr.NutrizioNino.Api.Models;

public partial class Nutrient
{
    /// <summary>Unità di misura canonica del nutriente; sostituisce la colonna denormalizzata UnitaMisura (single source of truth = FK).</summary>
    public virtual UnitOfMeasure? DefaultUnitOfMeasure { get; set; }
}
