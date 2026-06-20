using System.Security.Claims;
using System.Text.Json;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class UserPreferencesMapping
{
    private static readonly string[] _defaultChartNutrients = ["Energia", "Grassi", "Carboidrati", "Fibre", "Proteine"];

    private record ChartPreferencesResponse(IList<string> VisibleNutrients);
    private record UpdateChartPreferencesRequest(IList<string> VisibleNutrients);

    public static IEndpointRouteBuilder MapUserPreferencesEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/users/me")
            .WithTags("Users")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1)
            .RequireAuthorization();

        group.MapGet("chart-preferences", async (
            UserManager<ApplicationUser> userManager,
            ClaimsPrincipal principal,
            CancellationToken ct) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            var nutrients = user.NutrientChartPreferences is not null
                ? JsonSerializer.Deserialize<List<string>>(user.NutrientChartPreferences) ?? [.. _defaultChartNutrients]
                : [.. _defaultChartNutrients];

            return Results.Ok(new ChartPreferencesResponse(nutrients));
        })
        .Produces<ChartPreferencesResponse>()
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Recupera preferenze nutrienti grafico")
        .WithDescription("Restituisce i nutrienti selezionati dall'utente per il grafico simulazioni. Fallback ai 5 default se non configurate.")
        .WithName("GetChartPreferences");

        group.MapPut("chart-preferences", async (
            UserManager<ApplicationUser> userManager,
            ClaimsPrincipal principal,
            [FromBody] UpdateChartPreferencesRequest request,
            CancellationToken ct) =>
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return Results.Unauthorized();
            }

            user.NutrientChartPreferences = JsonSerializer.Serialize(request.VisibleNutrients);
            await userManager.UpdateAsync(user);
            return Results.NoContent();
        })
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .WithSummary("Salva preferenze nutrienti grafico")
        .WithDescription("Persiste i nutrienti selezionati dall'utente per il grafico simulazioni.")
        .WithName("UpdateChartPreferences");

        return endpoints;
    }
}
