using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class FoodVisionMapping
{
    private record ExtractNutrientsRequest(string Base64Image, string ProviderKey, string MediaType = "image/jpeg");

    public static IEndpointRouteBuilder MapFoodVisionEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/foods")
            .WithTags("Foods")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization();

        group.MapPost("extract-nutrients", async (
            VisionExtractionService service,
            [FromBody] ExtractNutrientsRequest request,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Base64Image))
            {
                return Results.BadRequest(new { error = "Base64Image è obbligatorio." });
            }

            if (string.IsNullOrWhiteSpace(request.ProviderKey))
            {
                return Results.BadRequest(new { error = "ProviderKey è obbligatorio." });
            }

            var result = await service.ExtractNutrientsAsync(request.Base64Image, request.MediaType, request.ProviderKey, ct);

            // Nessun nutriente estratto: l'immagine non è una tabella nutrizionale riconoscibile.
            if (result.Nutrients.Count == 0)
            {
                return Results.Problem(
                    detail: "Etichetta non riconosciuta. Verifica che l'immagine sia una tabella nutrizionale.",
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }

            return Results.Ok(result);
        })
        .RequireRateLimiting("vision")
        .Produces<ExtractionResultDto>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status429TooManyRequests)
        .WithSummary("Estrai nutrienti da immagine")
        .WithDescription("Invia un'immagine in base64 e il provider LLM scelto; riceve i nutrienti estratti con ExtractionStatus e il JSON grezzo del provider. Max 3 richieste al minuto per utente.")
        .WithName("ExtractNutrientsFromImage");

        return endpoints;
    }
}
