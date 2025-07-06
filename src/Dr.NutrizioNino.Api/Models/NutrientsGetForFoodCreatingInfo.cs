namespace Dr.NutrizioNino.Api.Models
{
    public class NutrientsGetForFoodCreatingInfo
    {
        public Guid NutrientId { get; set; }
        public string Name { get; set; }
        public int PositionOrder { get; set; }
        public Guid UnitOfMeasureId { get; set; }
        public decimal Quantity { get; set; }
    }
}
