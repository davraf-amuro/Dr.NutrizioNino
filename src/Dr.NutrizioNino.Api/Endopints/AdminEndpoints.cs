using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Services;
using TinyHelpers.AspNetCore.Extensions;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Dr.NutrizioNino.Api.Endopints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapsAdminEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/admin/users")
            .WithTags("Admin")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (AdminUserService service) =>
        {
            var users = await service.GetUsersAsync();
            return Results.Ok(users);
        })
        .WithName("GetUsers")
        .WithSummary("Lista tutti gli utenti")
        .Produces<IList<UserListItem>>(StatusCodes.Status200OK);

        group.MapPost("/", async (AdminUserService service, [FromBody] CreateUserRequest request) =>
        {
            var (success, errors) = await service.CreateUserAsync(request);
            if (!success)
                return Results.Problem(string.Join("; ", errors), statusCode: StatusCodes.Status400BadRequest);
            return Results.Created();
        })
        .WithName("CreateUser")
        .WithSummary("Registra un nuovo utente")
        .Produces(StatusCodes.Status201Created)
        .ProducesDefaultProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("{id:guid}/role", async (AdminUserService service, Guid id, [FromBody] ChangeRoleRequest request) =>
        {
            var (success, errors) = await service.ChangeRoleAsync(id, request.Role);
            if (!success)
                return Results.Problem(string.Join("; ", errors), statusCode: StatusCodes.Status400BadRequest);
            return Results.NoContent();
        })
        .WithName("ChangeUserRole")
        .WithSummary("Cambia il ruolo di un utente (User ↔ Admin)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesDefaultProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}
