namespace Dr.NutrizioNino.Api.Models
{
    public class FoodDashboard
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public decimal Quantity { get; set; }

        public string BrandDescription { get; set; }

        public decimal Calorie { get; set; }

        public string UnitOfMeasureDescription { get; set; }
        
        public string Abbreviation { get; set; }
    }
}
