namespace Dr.NutrizioNino.Models.Dto.Auth;

public record AddProfileEntryRequest(
    decimal? WeightKg,
    decimal? IdealWeightKg,
    decimal? HeightCm,
    string? Sex,
    string? Job
);
