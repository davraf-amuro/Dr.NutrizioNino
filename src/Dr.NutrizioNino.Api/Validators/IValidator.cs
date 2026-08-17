namespace Dr.NutrizioNino.Api.Validators;

/// <summary>Contratto di validazione per un input esterno: gli handler lo invocano prima di qualsiasi accesso ai dati.</summary>
public interface IValidator<T>
{
    /// <summary>Valida l'input e restituisce l'esito con l'elenco degli errori per campo.</summary>
    ValidationResult Validate(T input);
}

/// <summary>Esito di una validazione: gli errori sono nel formato atteso da TypedResults.ValidationProblem.</summary>
public record ValidationResult(bool IsValid, IDictionary<string, string[]> Errors)
{
    /// <summary>Esito valido, senza errori.</summary>
    public static ValidationResult Success() => new(true, new Dictionary<string, string[]>());

    /// <summary>Esito non valido con gli errori rilevati, indicizzati per nome campo.</summary>
    public static ValidationResult Failure(IDictionary<string, string[]> errors) => new(false, errors);
}
