namespace Dr.NutrizioNino.Models.Dto;

/// <summary>Fabbisogno nutrizionale dichiarato dall'utente (valore corrente, nessuno storico).</summary>
public record NutritionalTargetDto(
    decimal? KcalTarget,
    decimal? CarbsTarget,
    decimal? ProteinTarget,
    decimal? FatTarget,
    DateTime UpdatedAt
);

/// <summary>Richiesta di impostazione del fabbisogno: ogni campo è opzionale, ma se fornito deve essere &gt; 0.</summary>
public record SetNutritionalTargetDto(
    decimal? KcalTarget,
    decimal? CarbsTarget,
    decimal? ProteinTarget,
    decimal? FatTarget
);
