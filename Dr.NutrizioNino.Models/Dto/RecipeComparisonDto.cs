namespace Dr.NutrizioNino.Models.Dto;

public record CompareRecipeItem(
    Guid RecipeId,
    decimal QuantityGrams
);

public record CompareRecipesRequest(
    IList<CompareRecipeItem> Items
);

public record RecipeComparisonNutrientDto(
    Guid NutrientId,
    string Name,
    int PositionOrder,
    Guid UnitOfMeasureId,
    decimal Quantity
);

public record RecipeComparisonItemDto(
    Guid RecipeId,
    string Name,
    decimal BaseWeightGrams,
    decimal QuantityGrams,
    bool IsNutritionStale,
    IList<RecipeComparisonNutrientDto> Nutrients
);

public record RecipeComparisonDto(
    IList<RecipeComparisonItemDto> Items
);
