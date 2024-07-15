namespace Dr.NutrizioNino.Models.Dto
{
    public record NutrientDto(Guid Id, string Name, int PositionOrder, Guid DefaultUnitOfMeasureId, decimal DefaultQuantity);
}
