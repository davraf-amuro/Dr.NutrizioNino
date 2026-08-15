namespace Dr.NutrizioNino.Models.Dto;

public record RecipeDetailNutrientDto(
    Guid NutrientId,
    string Name,
    int PositionOrder,
    Guid UnitOfMeasureId,
    decimal Quantity
);

public record RecipeDetailIngredientDto(
    Guid FoodId,
    string FoodName,
    decimal QuantityGrams
);

public record RecipeDetailDto(
    Guid Id,
    string Name,
    decimal WeightGrams,
    IList<RecipeDetailIngredientDto> Ingredients,
    IList<RecipeDetailNutrientDto> Nutrients
);

public record RescaleRecipeRequest(decimal WeightGrams, bool Recalculate = false);
