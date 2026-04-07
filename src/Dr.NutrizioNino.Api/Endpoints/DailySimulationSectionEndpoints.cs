using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class DailySimulationSectionEndpoints
{
    public static IEndpointRouteBuilder MapsDailySimulationSectionEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/sections")
            .WithTags("Sections")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization();

        // GET / — lista sezioni (tutte, ordinate per DisplayOrder) — autenticato
        group.MapGet("", async (DailySimulationSectionService service, CancellationToken ct) =>
        {
            var items = await service.GetAllAsync(ct);
            return Results.Ok(items);
        })
            .WithName("GetSections")
            .WithSummary("Get all simulation sections")
            .Produces<IList<SimulationSectionDto>>(StatusCodes.Status200OK);

        // GET /active — solo sezioni attive — autenticato
        group.MapGet("active", async (DailySimulationSectionService service, CancellationToken ct) =>
        {
            var items = await service.GetActiveAsync(ct);
            return Results.Ok(items);
        })
            .WithName("GetActiveSections")
            .WithSummary("Get active simulation sections")
            .Produces<IList<SimulationSectionDto>>(StatusCodes.Status200OK);

        // POST / — crea sezione — AdminOnly
        group.MapPost("", async (DailySimulationSectionService service, CreateSimulationSectionDto dto, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return TypedResults.Problem(new ProblemDetails { Title = "Nome non valido", Status = 400, Detail = "Il nome è obbligatorio." });

            var id = await service.CreateAsync(dto.Name, ct);
            return Results.Created($"api/v1/sections/{id}", new { id });
        })
            .WithName("CreateSection")
            .WithSummary("Create a new simulation section")
            .Produces(StatusCodes.Status201Created)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization("AdminOnly");

        // PUT /{id} — rinomina sezione — AdminOnly
        group.MapPut("{id}", async (DailySimulationSectionService service, Guid id, UpdateSimulationSectionDto dto, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return TypedResults.Problem(new ProblemDetails { Title = "Nome non valido", Status = 400, Detail = "Il nome è obbligatorio." });

            var found = await service.UpdateAsync(id, dto.Name, ct);
            return found ? Results.Ok() : TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Sezione non trovata." });
        })
            .WithName("UpdateSection")
            .WithSummary("Rename a simulation section")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

        // DELETE /{id} — soft delete — AdminOnly
        group.MapDelete("{id}", async (DailySimulationSectionService service, Guid id, CancellationToken ct) =>
        {
            var found = await service.DeleteAsync(id, ct);
            return found ? Results.Ok() : TypedResults.Problem(new ProblemDetails { Title = "Not Found", Status = 404, Detail = "Sezione non trovata." });
        })
            .WithName("DeleteSection")
            .WithSummary("Soft-delete a simulation section")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminOnly");

        // PUT /reorder — riordina sezioni — AdminOnly
        group.MapPut("reorder", async (DailySimulationSectionService service, IList<SimulationSectionReorderItem> items, CancellationToken ct) =>
        {
            await service.ReorderAsync(items, ct);
            return Results.Ok();
        })
            .WithName("ReorderSections")
            .WithSummary("Reorder simulation sections")
            .Produces(StatusCodes.Status200OK)
            .RequireAuthorization("AdminOnly");

        return endpoints;
    }
}
