using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
            .Produces<IList<DishDashboardInfo>>(StatusCodes.Status200OK);

        group.MapGet("{id}", async (DishService service, Guid id, CancellationToken ct) =>
        {
            var result = await service.GetDishDetailAsync(id, ct);
            return result is not null
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Dish not found."
                });
        })
            .WithName("GetDishDetail")
            .WithSummary("Get dish detail")
            .WithDescription("Returns dish details with ingredients and nutrients.")
            .Produces<DishDetailDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapPost("", async (DishService service, CreateDishDto dto, CancellationToken ct) =>
        {
            if (await service.IsDishNameTakenAsync(dto.Name, ct))
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
            .WithDescription("Creates a dish by combining existing foods. Nutrients are calculated on the real dish weight (WeightGrams = sum of ingredient grams).")
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

        group.MapPost("{id}/recalculate", async (DishService service, Guid id, CancellationToken ct) =>
        {
            var (found, error) = await service.RecalculateDishAsync(id, ct);
            if (!found)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Dish not found."
                });
            }

            if (error is not null)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Ricalcolo non eseguito",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = error
                });
            }

            return Results.Ok();
        })
            .WithName("RecalculateDish")
            .WithSummary("Recalculate dish nutrition")
            .WithDescription("Recalculates calories and nutrients for the specified dish from its current ingredients. Clears IsNutritionStale.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound, StatusCodes.Status422UnprocessableEntity);

        group.MapPost("recalculate-stale", async (DishService service, CancellationToken ct) =>
        {
            var count = await service.RecalculateAllStaleDishesAsync(ct);
            return Results.Ok(new { recalculated = count });
        })
            .WithName("RecalculateAllStaleDishes")
            .WithSummary("Recalculate all stale dishes")
            .WithDescription("Recalculates all dishes with IsNutritionStale = true. Returns the count of updated dishes.")
            .Produces<object>(StatusCodes.Status200OK);

        group.MapPatch("{id}/quantity", async (DishService service, Guid id, RescaleDishRequest request, CancellationToken ct) =>
        {
            if (request.WeightGrams <= 0)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Peso non valido",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Il peso del piatto deve essere maggiore di zero."
                });
            }

            var (found, error) = await service.UpdateWeightAsync(id, request.WeightGrams, request.Recalculate, ct);
            if (!found)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Dish not found."
                });
            }

            if (error is not null)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Aggiornamento peso non eseguito",
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = error
                });
            }

            return Results.Ok();
        })
            .WithName("UpdateDishWeight")
            .WithSummary("Update dish weight")
            .WithDescription("Updates the dish weight. By default applies proportional rescaling (O(1), no ingredient access). Pass recalculate=true to fully recalculate nutrients from ingredients instead.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound, StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }
}
