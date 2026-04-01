using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.AspNetCore.Identity;

namespace Dr.NutrizioNino.Api.Services;

public class AdminUserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
{
    public async Task<IList<UserListItem>> GetUsersAsync()
    {
        var result = new List<UserListItem>();
        foreach (var user in userManager.Users.ToList())
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserListItem(user.Id, user.UserName!, user.Email!, user.DateOfBirth, roles.FirstOrDefault() ?? "User"));
        }
        return result;
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(CreateUserRequest request)
    {
        await EnsureRolesExistAsync();

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
            return (false, createResult.Errors.Select(e => e.Description));

        var role = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role;
        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
            return (false, roleResult.Errors.Select(e => e.Description));

        return (true, []);
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> ChangeRoleAsync(Guid userId, string newRole)
    {
        await EnsureRolesExistAsync();

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, ["Utente non trovato."]);

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);

        var result = await userManager.AddToRoleAsync(user, newRole);
        return result.Succeeded
            ? (true, [])
            : (false, result.Errors.Select(e => e.Description));
    }

    private async Task EnsureRolesExistAsync()
    {
        foreach (var role in new[] { "User", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = role, NormalizedName = role.ToUpper() });
        }
    }
}
