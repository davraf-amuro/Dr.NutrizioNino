using System.ComponentModel.DataAnnotations;

namespace Dr.NutrizioNino.Api.Infrastructure.Models
{
    public class NutrientsGetForFoodCreatingInfo
    {
        [Key]
        public Guid NutrientId { get; set; }
        public string Name { get; set; }
        public int PositionOrder { get; set; }
        public Guid UnitOfMeasureId { get; set; }
        public decimal Quantity { get; set; }
    }
}
