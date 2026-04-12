namespace Dr.NutrizioNino.Models.Dto;

public record ExtractedNutrientDto(
    string Name,
    decimal Value,
    string Unit,
    decimal? ConvertedValue,
    string? CanonicalUnit,
    Guid? MatchedNutrientId,
    float ConfidenceScore);
