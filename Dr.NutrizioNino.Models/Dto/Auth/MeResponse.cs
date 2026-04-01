namespace Dr.NutrizioNino.Models.Dto.Auth;

public record MeResponse(Guid Id, string UserName, string Email, DateOnly DateOfBirth, string Role);
