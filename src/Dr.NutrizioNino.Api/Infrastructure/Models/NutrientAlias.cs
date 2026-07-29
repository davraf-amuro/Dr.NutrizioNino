using System.ComponentModel.DataAnnotations.Schema;

namespace Dr.NutrizioNino.Api.Infrastructure.Models;

[Table("NutrientAlias")]
public class NutrientAlias
{
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("AiName")]
    public string AiName { get; set; } = string.Empty;

    [Column("NutrientId")]
    public Guid NutrientId { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
