using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Services;
using TinyHelpers.AspNetCore.Extensions;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class UserProfileEndpoints
{
    public static IEndpointRouteBuilder MapsUserProfileEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/users/me/profile")
            .WithTags("UserProfile")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization();

        group.MapGet("/", async (UserProfileService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var history = await service.GetHistoryAsync(userId, ct);
            return Results.Ok(history);
        })
        .WithName("GetProfileHistory")
        .WithSummary("Storico completo delle misurazioni dell'utente")
        .Produces<IList<ProfileEntryResponse>>(StatusCodes.Status200OK);

        group.MapGet("current", async (UserProfileService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var entry = await service.GetCurrentAsync(userId, ct);
            return entry is null ? Results.NotFound() : Results.Ok(entry);
        })
        .WithName("GetCurrentProfile")
        .WithSummary("Ultima misurazione registrata dall'utente")
        .Produces<ProfileEntryResponse>(StatusCodes.Status200OK)
        .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (UserProfileService service, ClaimsPrincipal user, [FromBody] AddProfileEntryRequest request, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var entry = await service.AddEntryAsync(userId, request, ct);
            return Results.Created($"api/v1/users/me/profile/{entry.Id}", entry);
        })
        .WithName("AddProfileEntry")
        .WithSummary("Aggiunge una nuova misurazione (peso, altezza, sesso, attività)")
        .Produces<ProfileEntryResponse>(StatusCodes.Status201Created);

        return endpoints;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;
        return Guid.Parse(sub);
    }
}
