namespace Dr.NutrizioNino.Models.Dto
{
    public record CreateFoodDto(string Name, string? Barcode, Guid? BrandId, int Calorie);

}
