using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Services;
using TinyHelpers.AspNetCore.Extensions;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Dr.NutrizioNino.Api.Endopints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapsAuthEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/auth")
            .WithTags("Auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1);

        group.MapPost("login", async (AuthService service, [FromBody] LoginRequest request) =>
        {
            var response = await service.LoginAsync(request);
            if (response is null)
                return Results.Problem("Credenziali non valide.", statusCode: StatusCodes.Status401Unauthorized);
            return Results.Ok(response);
        })
        .WithName("Login")
        .WithSummary("Esegui il login e ottieni il JWT")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesDefaultProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        group.MapGet("me", async (AuthService service, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);
            var me = await service.GetMeAsync(userId);
            return me is null ? Results.NotFound() : Results.Ok(me);
        })
        .WithName("GetMe")
        .WithSummary("Restituisce i dati dell'utente autenticato")
        .Produces<MeResponse>(StatusCodes.Status200OK)
        .ProducesDefaultProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        group.MapPatch("me/birthdate", async (AuthService service, ClaimsPrincipal user, [FromBody] UpdateBirthdateRequest request) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!);
            var ok = await service.UpdateBirthdateAsync(userId, request.DateOfBirth);
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateMyBirthdate")
        .WithSummary("Aggiorna la data di nascita dell'utente autenticato")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesDefaultProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        return endpoints;
    }
}
