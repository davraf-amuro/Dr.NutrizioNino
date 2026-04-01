namespace Dr.NutrizioNino.Models.Dto.Auth;

public record LoginResponse(string Token, DateTime ExpiresAt, string UserName, string Role);
