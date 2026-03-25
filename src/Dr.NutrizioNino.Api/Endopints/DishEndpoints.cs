using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endopints;

public static class DishEndpoints
{
    public static IEndpointRouteBuilder MapsDishesEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/dishes")
            .WithTags("Dishes")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1);

        group.MapGet("dashboard", async (DishService service, CancellationToken ct) =>
        {
            var result = await service.GetDishesDashboardAsync(ct);
            return Results.Ok(result);
        })
            .WithName("GetDishesDashboard")
            .WithSummary("Get dishes dashboard")
            .WithDescription("Returns the list of all dishes.")
            .Produces<IList<FoodDashboardInfo>>(StatusCodes.Status200OK);

        group.MapPost("", async (DishService service, CreateDishDto dto, CancellationToken ct) =>
        {
            if (await service.IsDishNameTakenAsync(dto.Name))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già un alimento con il nome \"{dto.Name}\"."
                });
            }

            var (id, error) = await service.CreateDishAsync(dto, ct);
            if (error is not null)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Errore creazione piatto",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = error
                });
            }

            return Results.Ok(id);
        })
            .WithName("CreateDish")
            .WithSummary("Create a dish")
            .WithDescription("Creates a dish by combining existing foods. Nutrients are calculated and normalised to 100g.")
            .Produces<Guid>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict);

        group.MapDelete("{id}", async (DishService service, Guid id, CancellationToken ct) =>
        {
            await service.DeleteDishAsync(id, ct);
            return Results.Ok();
        })
            .WithName("DeleteDish")
            .WithSummary("Delete a dish")
            .WithDescription("Deletes a dish and its ingredient list.")
            .Produces(StatusCodes.Status200OK);

        return endpoints;
    }
}
