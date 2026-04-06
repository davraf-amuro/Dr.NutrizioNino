using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.AspNetCore.Identity;

namespace Dr.NutrizioNino.Api.Services;

public class AdminUserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
{
    public async Task<IList<UserListItem>> GetUsersAsync(CancellationToken ct = default)
    {
        var adminsTask = userManager.GetUsersInRoleAsync("Admin");
        var usersTask = userManager.GetUsersInRoleAsync("User");
        await Task.WhenAll(adminsTask, usersTask);
        var admins = adminsTask.Result;
        var users = usersTask.Result;
        var adminIds = admins.Select(u => u.Id).ToHashSet();
        var allUsers = admins.Concat(users.Where(u => !adminIds.Contains(u.Id)));
        return allUsers
            .Select(u => new UserListItem(u.Id, u.UserName!, u.Email!, u.DateOfBirth,
                adminIds.Contains(u.Id) ? "Admin" : "User"))
            .ToList();
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> CreateUserAsync(CreateUserRequest request, CancellationToken ct = default)
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

    public async Task<UserListItem?> GetUserByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return null;
        var roles = await userManager.GetRolesAsync(user);
        return new UserListItem(user.Id, user.UserName!, user.Email!, user.DateOfBirth, roles.FirstOrDefault() ?? "User");
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, ["Utente non trovato."]);

        user.UserName = request.UserName;
        user.Email = request.Email;
        user.DateOfBirth = request.DateOfBirth;

        var result = await userManager.UpdateAsync(user);
        return result.Succeeded
            ? (true, [])
            : (false, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return (false, ["Utente non trovato."]);

        var result = await userManager.DeleteAsync(user);
        return result.Succeeded
            ? (true, [])
            : (false, result.Errors.Select(e => e.Description));
    }

    public async Task<(bool Success, IEnumerable<string> Errors)> ChangeRoleAsync(Guid userId, string newRole, CancellationToken ct = default)
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

    internal async Task EnsureRolesExistAsync()
    {
        foreach (var role in new[] { "User", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = role, NormalizedName = role.ToUpper() });
        }
    }
}
