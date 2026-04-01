namespace Dr.NutrizioNino.Models.Dto.Auth;

public record AddProfileEntryRequest(
    decimal? WeightKg,
    decimal? HeightCm,
    string? Sex,
    string? Job
);
