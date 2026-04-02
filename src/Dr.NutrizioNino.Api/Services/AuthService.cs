using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Dr.NutrizioNino.Api.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration)
{
    public async Task<LoginResult?> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByNameAsync(request.UserName);
        if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
            return null;

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        var (token, expiresAt) = GenerateJwt(user, role);
        return new LoginResult(token, expiresAt, user.UserName!, role);
    }

    public async Task<MeResponse?> GetMeAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "User";

        return new MeResponse(user.Id, user.UserName!, user.Email!, user.DateOfBirth, role);
    }

    public async Task<bool> UpdateBirthdateAsync(Guid userId, DateOnly dateOfBirth)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.DateOfBirth = dateOfBirth;
        var result = await userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    private (string token, DateTime expiresAt) GenerateJwt(ApplicationUser user, string role)
    {
        var secret = configuration["Jwt:Secret"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

public record LoginResult(string RawToken, DateTime ExpiresAt, string UserName, string Role);
