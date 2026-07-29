using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Services.Vision;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class VisionProvidersMapping
{
    public static IEndpointRouteBuilder MapVisionProvidersEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/vision")
            .WithTags("Vision")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization();

        group.MapGet("providers", (VisionProviderFactory factory) =>
            Results.Ok(factory.GetAll()))
        .Produces<IReadOnlyList<VisionProviderInfo>>()
        .WithSummary("Lista provider LLM disponibili")
        .WithDescription("Restituisce i provider LLM configurati nel sistema per l'estrazione nutrienti da immagine.")
        .WithName("GetVisionProviders");

        return endpoints;
    }
}
