namespace Dr.NutrizioNino.Models.Dto
{
    public record FoodDto(Guid Id, string Name, decimal Quantity, string? Barcode, Guid? BrandId, int Calorie);
}
