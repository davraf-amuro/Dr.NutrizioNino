namespace Dr.NutrizioNino.Models.Dto
{
    public record NutrientInfo(Guid Id, string Name, int PositionOrder, Guid DefaultUnitOfMeasureId, decimal DefaultQuantity);
}
