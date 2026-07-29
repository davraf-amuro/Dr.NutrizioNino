using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class HealthMapping
{
    /// <summary>
    /// Mappa gli endpoint di stato del sistema: lettura stato DB e retry connessione+seed.
    /// Anonimi perché l'autenticazione dipende dal database e devono restare raggiungibili a DB down.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/status")
            .WithTags("Status")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .AllowAnonymous();

        group.MapGet("/", GetStatusAsync)
            .Produces<DatabaseStatusResponse>(StatusCodes.Status200OK)
            .WithSummary("Stato del database")
            .WithDescription("Verifica live la raggiungibilità del database e restituisce lo stato corrente, l'ultimo errore e l'istante del controllo.")
            .WithName("GetStatus");

        group.MapPost("retry-database", RetryDatabaseAsync)
            .Produces<DatabaseStatusResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)
            .WithSummary("Ritenta connessione al database")
            .WithDescription("Ritenta la connessione al database ed esegue il seed dei ruoli. 200 se il database è ora pronto, 503 se ancora non raggiungibile.")
            .WithName("RetryDatabase");

        return endpoints;
    }

    /// <summary>Restituisce lo stato live del database (sempre 200, anche a DB down).</summary>
    private static async Task<IResult> GetStatusAsync(DatabaseStartupService dbStartup, CancellationToken ct)
    {
        await dbStartup.CheckConnectionAsync(ct);
        return TypedResults.Ok(new DatabaseStatusResponse(dbStartup.IsDatabaseReady, dbStartup.LastError, dbStartup.LastCheckedUtc));
    }

    /// <summary>Ritenta connessione+seed; 200 se pronto, 503 ProblemDetails se ancora down.</summary>
    private static async Task<IResult> RetryDatabaseAsync(DatabaseStartupService dbStartup, CancellationToken ct)
    {
        var ready = await dbStartup.TryInitializeAsync(ct);
        if (ready)
        {
            return TypedResults.Ok(new DatabaseStatusResponse(true, null, dbStartup.LastCheckedUtc));
        }

        return TypedResults.Problem(new ProblemDetails
        {
            Title = "Base Dati non pronta",
            Detail = dbStartup.LastError ?? "Il database non è al momento raggiungibile.",
            Status = StatusCodes.Status503ServiceUnavailable
        });
    }
}

/// <summary>Stato di disponibilità del database esposto al frontend.</summary>
public record DatabaseStatusResponse(bool DatabaseReady, string? LastError, DateTimeOffset? LastCheckedUtc);
