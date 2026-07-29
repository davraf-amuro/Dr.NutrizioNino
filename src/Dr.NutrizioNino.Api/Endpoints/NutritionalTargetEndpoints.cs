using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class NutritionalTargetEndpoints
{
    public static IEndpointRouteBuilder MapNutritionalTargetEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/users/me/nutritional-target")
            .WithTags("NutritionalTarget")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization();

        // GET / — fabbisogno corrente dell'utente autenticato
        group.MapGet("/", async (NutritionalTargetService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            if (!userId.HasValue)
            {
                return Results.Forbid();
            }

            var target = await service.GetAsync(userId.Value, ct);
            return target is null
                ? TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Fabbisogno non impostato." })
                : Results.Ok(target);
        })
            .WithName("GetNutritionalTarget")
            .WithSummary("Get user's current nutritional target")
            .WithDescription("Returns the authenticated user's declared nutritional target (kcal/carbs/protein/fat). 404 if never set.")
            .Produces<NutritionalTargetDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        // PUT / — imposta/aggiorna il fabbisogno (upsert)
        group.MapPut("/", async (NutritionalTargetService service, SetNutritionalTargetDto request, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            if (!userId.HasValue)
            {
                return Results.Forbid();
            }

            // Ogni campo fornito deve essere > 0 — 0 o negativo non ha senso come fabbisogno dichiarato
            if (request.KcalTarget <= 0 || request.CarbsTarget <= 0 || request.ProteinTarget <= 0 || request.FatTarget <= 0)
            {
                return TypedResults.Problem(new ProblemDetails { Title = "Valore non valido", Status = 400, Detail = "Ogni valore di fabbisogno, se fornito, deve essere maggiore di zero." });
            }

            var updated = await service.UpsertAsync(userId.Value, request, ct);
            return Results.Ok(updated);
        })
            .WithName("SetNutritionalTarget")
            .WithSummary("Set user's nutritional target")
            .WithDescription("Creates or updates the authenticated user's nutritional target. Returns 400 if any provided value is not greater than zero.")
            .Produces<NutritionalTargetDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}
