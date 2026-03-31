using System.ComponentModel.DataAnnotations.Schema;

namespace Dr.NutrizioNino.Api.Infrastructure.Models;

[Table("Dishes_Dashboard")]
public class DishDashboardInfo
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public decimal Quantity { get; set; }

    public decimal Calorie { get; set; }

    public string? UnitOfMeasureDescription { get; set; }

    public string? Abbreviation { get; set; }

    public bool IsNutritionStale { get; set; }

    public DateTime? NutrientsCalculatedAt { get; set; }
}
