using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Services;
using TinyHelpers.AspNetCore.Extensions;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapsAuthEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet, IWebHostEnvironment env)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/auth")
            .WithTags("Auth")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1);

        group.MapPost("login", async (AuthService service, [FromBody] LoginRequest request) =>
        {
            var result = await service.LoginAsync(request);
            if (result is null)
                return Results.Problem("Credenziali non valide.", statusCode: StatusCodes.Status401Unauthorized);

            return Results.Ok(new LoginResponse(result.RawToken, result.UserName, result.Role));
        })
        .WithName("Login")
        .WithSummary("Esegui il login e imposta il cookie di autenticazione")
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .ProducesDefaultProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();

        group.MapPost("logout", () =>
        {
            return Results.NoContent();
        })
        .WithName("Logout")
        .WithSummary("Cancella il cookie di autenticazione")
        .Produces(StatusCodes.Status204NoContent)
        .AllowAnonymous();

        group.MapGet("me", async (AuthService service, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
                return Results.Problem("Utente non autenticato.", statusCode: StatusCodes.Status401Unauthorized);
            var me = await service.GetMeAsync(userId.Value);
            return me is null ? Results.NotFound() : Results.Ok(me);
        })
        .WithName("GetMe")
        .WithSummary("Restituisce i dati dell'utente autenticato")
        .Produces<MeResponse>(StatusCodes.Status200OK)
        .ProducesDefaultProblem(StatusCodes.Status404NotFound)
        .RequireAuthorization();

        group.MapPatch("me/birthdate", async (AuthService service, ClaimsPrincipal user, [FromBody] UpdateBirthdateRequest request) =>
        {
            var userId = user.GetUserId();
            if (userId is null)
                return Results.Problem("Utente non autenticato.", statusCode: StatusCodes.Status401Unauthorized);
            var ok = await service.UpdateBirthdateAsync(userId.Value, request.DateOfBirth);
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
