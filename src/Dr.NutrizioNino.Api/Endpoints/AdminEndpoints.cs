using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Services;
using TinyHelpers.AspNetCore.Extensions;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapsAdminEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/admin/users")
            .WithTags("Admin")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization("AdminOnly");

        group.MapGet("/", async (AdminUserService service, CancellationToken ct) =>
        {
            var users = await service.GetUsersAsync(ct);
            return Results.Ok(users);
        })
        .WithName("GetUsers")
        .WithSummary("Lista tutti gli utenti")
        .Produces<IList<UserListItem>>(StatusCodes.Status200OK);

        group.MapPost("/", async (AdminUserService service, [FromBody] CreateUserRequest request, CancellationToken ct) =>
        {
            var (success, errors) = await service.CreateUserAsync(request, ct);
            if (!success)
                return Results.Problem(string.Join("; ", errors), statusCode: StatusCodes.Status400BadRequest);
            return Results.Created();
        })
        .WithName("CreateUser")
        .WithSummary("Registra un nuovo utente")
        .Produces(StatusCodes.Status201Created)
        .ProducesDefaultProblem(StatusCodes.Status400BadRequest);

        group.MapGet("{id:guid}", async (AdminUserService service, Guid id, CancellationToken ct) =>
        {
            var user = await service.GetUserByIdAsync(id, ct);
            return user is not null
                ? Results.Ok(user)
                : Results.Problem("Utente non trovato.", statusCode: StatusCodes.Status404NotFound);
        })
        .WithName("GetUserById")
        .WithSummary("Restituisce un utente per id")
        .Produces<UserListItem>(StatusCodes.Status200OK)
        .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapPut("{id:guid}", async (AdminUserService service, Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct) =>
        {
            var (success, errors) = await service.UpdateUserAsync(id, request, ct);
            if (!success)
                return Results.Problem(string.Join("; ", errors), statusCode: StatusCodes.Status400BadRequest);
            return Results.NoContent();
        })
        .WithName("UpdateUser")
        .WithSummary("Aggiorna username, email e data di nascita di un utente")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesDefaultProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("{id:guid}", async (AdminUserService service, Guid id, CancellationToken ct) =>
        {
            var (success, errors) = await service.DeleteUserAsync(id, ct);
            if (!success)
                return Results.Problem(string.Join("; ", errors), statusCode: StatusCodes.Status400BadRequest);
            return Results.NoContent();
        })
        .WithName("DeleteUser")
        .WithSummary("Elimina un utente")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesDefaultProblem(StatusCodes.Status400BadRequest);

        group.MapPatch("{id:guid}/role", async (AdminUserService service, Guid id, [FromBody] ChangeRoleRequest request, CancellationToken ct) =>
        {
            var (success, errors) = await service.ChangeRoleAsync(id, request.Role, ct);
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
