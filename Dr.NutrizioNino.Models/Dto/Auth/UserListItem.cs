namespace Dr.NutrizioNino.Models.Dto.Auth;

public record UserListItem(Guid Id, string UserName, string Email, DateOnly DateOfBirth, string Role);
