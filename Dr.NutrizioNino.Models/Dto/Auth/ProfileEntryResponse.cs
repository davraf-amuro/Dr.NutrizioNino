namespace Dr.NutrizioNino.Models.Dto.Auth;

public record ProfileEntryResponse(
    Guid Id,
    DateTime RecordedAt,
    decimal? WeightKg,
    decimal? HeightCm,
    string? Sex,
    string? Job
);
