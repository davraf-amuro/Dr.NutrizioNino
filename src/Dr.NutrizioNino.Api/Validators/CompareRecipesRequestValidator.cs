using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Validators;

/// <summary>Valida la richiesta di confronto ricette prima di qualsiasi lettura dal database.</summary>
public class CompareRecipesRequestValidator : IValidator<CompareRecipesRequest>
{
    private const int MinItems = 2;
    private const int MaxItems = 3;

    // Quantity su Recipes_Nutrients è numeric(6,2): oltre questa soglia il riscalamento può eccedere il tipo.
    private const decimal MaxQuantityGrams = 10000m;

    /// <summary>Verifica numero, unicità e quantità degli elementi da confrontare.</summary>
    public ValidationResult Validate(CompareRecipesRequest input)
    {
        var errors = new Dictionary<string, string[]>();

        if (input.Items is null || input.Items.Count < MinItems || input.Items.Count > MaxItems)
        {
            errors["items"] = [$"Il confronto richiede da {MinItems} a {MaxItems} ricette."];
            return ValidationResult.Failure(errors);
        }

        if (input.Items.Any(i => i.RecipeId == Guid.Empty))
        {
            errors["items.recipeId"] = ["Ogni elemento deve indicare una ricetta valida."];
        }

        if (input.Items.Select(i => i.RecipeId).Distinct().Count() != input.Items.Count)
        {
            errors["items.recipeId.duplicated"] = ["Le ricette da confrontare devono essere diverse tra loro."];
        }

        if (input.Items.Any(i => i.QuantityGrams <= 0 || i.QuantityGrams > MaxQuantityGrams))
        {
            errors["items.quantityGrams"] = [$"La quantità deve essere maggiore di zero e non superiore a {MaxQuantityGrams:0} grammi."];
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
