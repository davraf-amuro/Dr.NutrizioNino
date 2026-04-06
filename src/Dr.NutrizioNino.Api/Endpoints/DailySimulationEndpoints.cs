using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class DailySimulationEndpoints
{
    public static IEndpointRouteBuilder MapDailySimulationEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/daily-simulations")
            .WithTags("DailySimulations")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization();

        // GET / — lista simulazioni dell'utente
        group.MapGet("", async (DailySimulationService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            if (!userId.HasValue) return Results.Forbid();
            var items = await service.GetUserSimulationsAsync(userId.Value, ct);
            return Results.Ok(items);
        })
            .WithName("GetDailySimulations")
            .WithSummary("Get user's daily simulations")
            .Produces<IList<DailySimulationListItemDto>>(StatusCodes.Status200OK);

        // POST / — crea simulazione vuota
        group.MapPost("", async (DailySimulationService service, CreateDailySimulationDto dto, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            if (!userId.HasValue) return Results.Forbid();
            var id = await service.CreateSimulationAsync(dto, userId.Value, ct);
            return Results.Created($"api/v1/daily-simulations/{id}", new { id });
        })
            .WithName("CreateDailySimulation")
            .WithSummary("Create a new daily simulation")
            .Produces(StatusCodes.Status201Created)
            .ProducesDefaultProblem();

        // GET /{id} — dettaglio simulazione
        group.MapGet("{id}", async (DailySimulationService service, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            if (ownerId is null)
                return TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Simulazione non trovata." });

            var callerId = user.GetUserId();
            if (ownerId != callerId) return Results.Forbid();

            var detail = await service.GetSimulationDetailAsync(id, ct);
            return Results.Ok(detail);
        })
            .WithName("GetDailySimulationDetail")
            .WithSummary("Get daily simulation detail")
            .Produces<DailySimulationDetailDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        // PUT /{id} — rinomina
        group.MapPut("{id}", async (DailySimulationService service, Guid id, RenameDailySimulationDto dto, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            if (ownerId is null)
                return TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Simulazione non trovata." });

            var callerId = user.GetUserId();
            if (ownerId != callerId) return Results.Forbid();

            await service.RenameSimulationAsync(id, dto.Name, ct);
            return Results.Ok();
        })
            .WithName("RenameDailySimulation")
            .WithSummary("Rename a daily simulation")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        // DELETE /{id} — elimina
        group.MapDelete("{id}", async (DailySimulationService service, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            if (ownerId is null)
                return TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Simulazione non trovata." });

            var callerId = user.GetUserId();
            if (ownerId != callerId) return Results.Forbid();

            await service.DeleteSimulationAsync(id, ct);
            return Results.Ok();
        })
            .WithName("DeleteDailySimulation")
            .WithSummary("Delete a daily simulation")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        // POST /{id}/clone — clona simulazione
        group.MapPost("{id}/clone", async (DailySimulationService service, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            if (ownerId is null)
                return TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Simulazione non trovata." });

            var callerId = user.GetUserId();
            if (ownerId != callerId) return Results.Forbid();

            var cloneId = await service.CloneSimulationAsync(id, callerId!.Value, ct);
            return Results.Created($"api/v1/daily-simulations/{cloneId}", new { id = cloneId });
        })
            .WithName("CloneDailySimulation")
            .WithSummary("Clone a daily simulation")
            .Produces(StatusCodes.Status201Created)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        // POST /{id}/entries — aggiunge voce
        group.MapPost("{id}/entries", async (DailySimulationService service, Guid id, AddSimulationEntryDto dto, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            if (ownerId is null)
                return TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Simulazione non trovata." });

            var callerId = user.GetUserId();
            if (ownerId != callerId) return Results.Forbid();

            if (dto.QuantityGrams <= 0)
                return TypedResults.Problem(new ProblemDetails { Title = "Quantità non valida", Status = 400, Detail = "La quantità deve essere maggiore di zero." });

            var (entryId, error) = await service.AddEntryAsync(id, dto, ct);
            if (error is not null)
                return TypedResults.Problem(new ProblemDetails { Title = "Sorgente non trovata", Status = 404, Detail = error });

            return Results.Created($"api/v1/daily-simulations/{id}/entries/{entryId}", new { id = entryId });
        })
            .WithName("AddSimulationEntry")
            .WithSummary("Add an entry to a daily simulation section")
            .Produces(StatusCodes.Status201Created)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound);

        // PUT /{id}/entries/{entryId} — modifica quantità
        group.MapPut("{id}/entries/{entryId}", async (DailySimulationService service, Guid id, Guid entryId, UpdateEntryQuantityDto dto, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            if (ownerId is null)
                return TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Simulazione non trovata." });

            var callerId = user.GetUserId();
            if (ownerId != callerId) return Results.Forbid();

            if (dto.QuantityGrams <= 0)
                return TypedResults.Problem(new ProblemDetails { Title = "Quantità non valida", Status = 400, Detail = "La quantità deve essere maggiore di zero." });

            var (found, error) = await service.UpdateEntryQuantityAsync(id, entryId, dto.QuantityGrams, ct);
            if (!found)
                return TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Voce non trovata." });

            if (error is not null)
                return TypedResults.Problem(new ProblemDetails { Title = "Ricalcolo non eseguito", Status = 422, Detail = error });

            return Results.Ok();
        })
            .WithName("UpdateSimulationEntryQuantity")
            .WithSummary("Update entry quantity in a daily simulation")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound, StatusCodes.Status422UnprocessableEntity);

        // DELETE /{id}/entries/{entryId} — rimuove voce
        group.MapDelete("{id}/entries/{entryId}", async (DailySimulationService service, Guid id, Guid entryId, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            if (ownerId is null)
                return TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Simulazione non trovata." });

            var callerId = user.GetUserId();
            if (ownerId != callerId) return Results.Forbid();

            var deleted = await service.DeleteEntryAsync(id, entryId, ct);
            return deleted ? Results.Ok() : TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Voce non trovata." });
        })
            .WithName("DeleteSimulationEntry")
            .WithSummary("Remove an entry from a daily simulation")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        // GET /compare?id1=&id2= — confronto nutrienti
        group.MapGet("compare", async (DailySimulationService service, Guid id1, Guid id2, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            if (!userId.HasValue) return Results.Forbid();

            var result = await service.CompareAsync(id1, id2, userId.Value, ct);
            return result is not null
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Una o entrambe le simulazioni non trovate." });
        })
            .WithName("CompareDailySimulations")
            .WithSummary("Compare nutrients between two daily simulations")
            .Produces<DailySimulationCompareDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }
}
