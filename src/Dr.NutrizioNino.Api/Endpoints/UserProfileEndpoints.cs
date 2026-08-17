using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Api.Validators;
using Dr.NutrizioNino.Models.Dto.Auth;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

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

        group.MapPost("/", async (UserProfileService service, IValidator<AddProfileEntryRequest> validator, ClaimsPrincipal user, [FromBody] AddProfileEntryRequest request, CancellationToken ct) =>
        {
            // Valido il body prima di qualsiasi scrittura
            var validation = validator.Validate(request);
            if (!validation.IsValid)
            {
                return Results.ValidationProblem(validation.Errors);
            }

            var userId = GetUserId(user);
            var entry = await service.AddEntryAsync(userId, request, ct);
            return Results.Created($"api/v1/users/me/profile/{entry.Id}", entry);
        })
        .WithName("AddProfileEntry")
        .WithSummary("Aggiunge una nuova misurazione (peso, peso ideale, altezza, sesso, attività)")
        .WithDescription("Registra una misurazione e, se peso e peso ideale sono presenti, calcola e salva la scheda Sciaudone corrispondente. Ritorna 400 se il body non è valido.")
        .Produces<ProfileEntryResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("sciaudone", async (SciaudoneCardService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var card = await service.GetCurrentAsync(userId, ct);
            return card is null ? Results.NotFound() : Results.Ok(card);
        })
        .WithName("GetCurrentSciaudoneCard")
        .WithSummary("Scheda Sciaudone corrente dell'utente")
        .WithDescription("Restituisce la scheda calcolata sull'ultima misurazione utile. 404 se l'utente non ha ancora una scheda. I valori sono una stima generica, non una prescrizione nutrizionale.")
        .Produces<SciaudoneCardDto>(StatusCodes.Status200OK)
        .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapGet("sciaudone/history", async (SciaudoneCardService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = GetUserId(user);
            var cards = await service.GetHistoryAsync(userId, ct);
            return Results.Ok(cards);
        })
        .WithName("GetSciaudoneCardHistory")
        .WithSummary("Storico delle schede Sciaudone dell'utente")
        .WithDescription("Restituisce tutte le schede calcolate per l'utente, dalla più recente alla più vecchia.")
        .Produces<IList<SciaudoneCardDto>>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)!;
        return Guid.Parse(sub);
    }
}
