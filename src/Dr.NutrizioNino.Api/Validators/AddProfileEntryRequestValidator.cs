using Dr.NutrizioNino.Models.Dto.Auth;

namespace Dr.NutrizioNino.Api.Validators;

/// <summary>Valida la misurazione di profilo prima della scrittura e del calcolo della scheda Sciaudone.</summary>
public class AddProfileEntryRequestValidator : IValidator<AddProfileEntryRequest>
{
    // UserProfileEntries usa numeric(5,2) per pesi e altezza: oltre questa soglia la scrittura eccede il tipo.
    private const decimal MaxMeasure = 999.99m;

    // Colonna Sex: nvarchar(1)
    private static readonly string[] _allowedSex = ["M", "F"];

    // Colonna Job: nvarchar(20)
    private static readonly string[] _allowedJob = ["sedentario", "moderato", "attivo", "molto_attivo"];

    /// <summary>Verifica i valori opzionali della misurazione: se presenti devono essere positivi e nei domini ammessi.</summary>
    public ValidationResult Validate(AddProfileEntryRequest input)
    {
        var errors = new Dictionary<string, string[]>();

        if (IsOutOfRange(input.WeightKg))
        {
            errors["weightKg"] = [$"Il peso deve essere maggiore di zero e non superiore a {MaxMeasure:0.##} kg."];
        }

        if (IsOutOfRange(input.IdealWeightKg))
        {
            errors["idealWeightKg"] = [$"Il peso ideale deve essere maggiore di zero e non superiore a {MaxMeasure:0.##} kg."];
        }

        if (IsOutOfRange(input.HeightCm))
        {
            errors["heightCm"] = [$"L'altezza deve essere maggiore di zero e non superiore a {MaxMeasure:0.##} cm."];
        }

        if (input.Sex is not null && !_allowedSex.Contains(input.Sex))
        {
            errors["sex"] = [$"Valori ammessi: {string.Join(", ", _allowedSex)}."];
        }

        if (input.Job is not null && !_allowedJob.Contains(input.Job))
        {
            errors["job"] = [$"Valori ammessi: {string.Join(", ", _allowedJob)}."];
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }

    /// <summary>Una misura opzionale è valida se assente, oppure positiva e entro il tipo della colonna.</summary>
    private static bool IsOutOfRange(decimal? value) =>
        value is { } measure && (measure <= 0 || measure > MaxMeasure);
}
