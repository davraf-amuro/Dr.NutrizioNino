namespace Dr.NutrizioNino.Models.Dto;

public record DishDetailNutrientDto(
    Guid NutrientId,
    string Name,
    int PositionOrder,
    Guid UnitOfMeasureId,
    decimal Quantity
);

public record DishDetailIngredientDto(
    Guid FoodId,
    string FoodName,
    decimal QuantityGrams
);

public record DishDetailDto(
    Guid Id,
    string Name,
    decimal WeightGrams,
    IList<DishDetailIngredientDto> Ingredients,
    IList<DishDetailNutrientDto> Nutrients
);

public record RescaleDishRequest(decimal WeightGrams, bool Recalculate = false);
