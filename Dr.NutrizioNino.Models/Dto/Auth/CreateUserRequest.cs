namespace Dr.NutrizioNino.Models.Dto.Auth;

public record CreateUserRequest(
    string UserName,
    string Email,
    string Password,
    DateOnly DateOfBirth,
    string Role = "User"
);
