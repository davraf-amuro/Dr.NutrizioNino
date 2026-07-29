using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class NutrientAliasMapping
{
    private record SaveAliasRequest(string AiName, Guid NutrientId);

    public static IEndpointRouteBuilder MapNutrientAliasEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/nutrients")
            .WithTags("Nutrients")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization();

        group.MapPost("aliases", async (
            [FromBody] SaveAliasRequest request,
            DrRepository repository,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.AiName) || request.AiName.Length > 200)
            {
                return Results.BadRequest(new { error = "AiName obbligatorio, max 200 caratteri." });
            }

            if (request.NutrientId == Guid.Empty)
            {
                return Results.BadRequest(new { error = "NutrientId non valido." });
            }

            // Verifica che il nutriente esista
            var nutrient = await repository.GetNutrientsAsync(
                n => n.Id == request.NutrientId ? n.Id : (Guid?)null, ct).ConfigureAwait(false);
            if (!nutrient.Any(id => id.HasValue))
            {
                return Results.NotFound(new { error = "Nutriente non trovato." });
            }

            var aiName = request.AiName.Trim();

            // Idempotenza: se l'alias esiste già (UNIQUE su AiName) restituisci quello esistente invece di un 500
            var existing = await repository.GetAliasByAiNameAsync(aiName, ct).ConfigureAwait(false);
            if (existing is not null)
            {
                return Results.Created($"/api/v1/nutrients/aliases/{existing.Id}", existing.Id);
            }

            var alias = new NutrientAlias
            {
                Id = Guid.NewGuid(),
                AiName = aiName,
                NutrientId = request.NutrientId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await repository.SaveAliasAsync(alias, ct).ConfigureAwait(false);
            }
            catch (DbUpdateException)
            {
                // Race condition: due POST concorrenti sullo stesso AiName → recupera l'alias vincente
                var winner = await repository.GetAliasByAiNameAsync(aiName, ct).ConfigureAwait(false);
                if (winner is not null)
                {
                    return Results.Created($"/api/v1/nutrients/aliases/{winner.Id}", winner.Id);
                }

                throw;
            }

            return Results.Created($"/api/v1/nutrients/aliases/{alias.Id}", alias.Id);
        })
        .RequireRateLimiting("aliases")
        .Produces<Guid>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status429TooManyRequests)
        .WithSummary("Salva alias nutriente AI")
        .WithDescription("Associa un nome estratto dall'AI a un nutriente canonico nel database. L'alias è globale e condiviso tra tutti gli utenti.")
        .WithName("SaveNutrientAlias");

        return endpoints;
    }
}
