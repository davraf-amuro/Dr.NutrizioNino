namespace Dr.NutrizioNino.Api.Models
{
    public class NutrientsGetForFoodCreatingInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int PositionOrder { get; set; }
        public Guid FoodId { get; set; }
        public Guid NutrientId { get; set; }
        public Guid UnitOfMeasureId { get; set; }
        public double Quantity { get; set; }
    }
}
