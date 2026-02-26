namespace Dr.NutrizioNino.Api.Infrastructure.Models;

public record NutrientInfo(Guid Id, string Name, int PositionOrder, Guid DefaultUnitOfMeasureId, decimal DefaultQuantity);
