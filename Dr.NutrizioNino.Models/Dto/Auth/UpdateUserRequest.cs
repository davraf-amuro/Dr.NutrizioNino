namespace Dr.NutrizioNino.Models.Dto.Auth;

public record UpdateUserRequest(
    string UserName,
    string Email,
    DateOnly DateOfBirth
);
