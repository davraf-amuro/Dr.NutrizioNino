namespace Dr.NutrizioNino.Models.Dto;

public record CreateNutrientDto(
    string Name,
    Guid? DefaultUnitOfMeasureId = null,
    int PositionOrder = 0,
    decimal DefaultQuantity = 0);
