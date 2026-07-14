namespace Dr.NutrizioNino.Models.Dto;

public record ExtractionResultDto(IList<ExtractedNutrientDto> Nutrients, string RawJson);
