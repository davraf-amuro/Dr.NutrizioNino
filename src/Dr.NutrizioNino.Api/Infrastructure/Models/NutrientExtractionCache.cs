using System.ComponentModel.DataAnnotations.Schema;

namespace Dr.NutrizioNino.Api.Infrastructure.Models;

[Table("NutrientExtractionCache")]
public class NutrientExtractionCache
{
    [Column("Id")]
    public Guid Id { get; set; }

    [Column("ImageHash")]
    public string ImageHash { get; set; } = string.Empty;

    [Column("ExtractedJson")]
    public string ExtractedJson { get; set; } = string.Empty;

    [Column("ConfidenceScore")]
    public float ConfidenceScore { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
