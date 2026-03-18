using System.ComponentModel.DataAnnotations.Schema;

namespace Dr.NutrizioNino.Api.Infrastructure.Models;

[Table("Foods_Dashboard")]
public class FoodDashboardInfo
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? Barcode { get; set; }

    public decimal Quantity { get; set; }

    public string? BrandDescription { get; set; }

    public decimal Calorie { get; set; }

    public string? UnitOfMeasureDescription { get; set; }

    public string? Abbreviation { get; set; }
}
