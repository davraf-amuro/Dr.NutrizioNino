using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class RecipeEndpoints
{
    private record RecipeDashboardResponse(
        Guid Id, string? Name, decimal Quantity, decimal Calorie,
        string? UnitOfMeasureDescription, string? Abbreviation,
        bool IsNutritionStale, DateTime? NutrientsCalculatedAt,
        bool IsOwner);

    public static IEndpointRouteBuilder MapsRecipesEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/recipes")
            .WithTags("Recipes")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1);

        group.MapGet("dashboard", async (RecipeService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var items = await service.GetRecipesDashboardAsync(ct);
            var userId = user.GetUserId();
            var result = items.Select(r => new RecipeDashboardResponse(
                r.Id, r.Name, r.Quantity, r.Calorie,
                r.UnitOfMeasureDescription, r.Abbreviation,
                r.IsNutritionStale, r.NutrientsCalculatedAt,
                IsOwner: userId.HasValue && r.OwnerId.HasValue && r.OwnerId == userId)).ToList();
            return Results.Ok(result);
        })
            .WithName("GetRecipesDashboard")
            .WithSummary("Get recipes dashboard")
            .WithDescription("Returns the list of all recipes.")
            .Produces<IList<RecipeDashboardInfo>>(StatusCodes.Status200OK);

        group.MapGet("{id}", async (RecipeService service, Guid id, CancellationToken ct) =>
        {
            var result = await service.GetRecipeDetailAsync(id, ct);
            return result is not null
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Recipe not found."
                });
        })
            .WithName("GetRecipeDetail")
            .WithSummary("Get recipe detail")
            .WithDescription("Returns recipe details with ingredients and nutrients.")
            .Produces<RecipeDetailDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapPost("", async (RecipeService service, CreateRecipeDto dto, ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (await service.IsRecipeNameTakenAsync(dto.Name, ct))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già un alimento con il nome \"{dto.Name}\"."
                });
            }

            var ownerId = user.GetUserId();
            var (detail, error) = await service.CreateRecipeAsync(dto, ownerId, ct);
            if (error is not null)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Errore creazione ricetta",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = error
                });
            }

            return Results.Ok(detail);
        })
            .WithName("CreateRecipe")
            .WithSummary("Create a recipe")
            .WithDescription("Creates a recipe by combining existing foods. Returns the complete recipe detail including calculated nutrients.")
            .Produces<RecipeDetailDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict)
            .RequireAuthorization();

        group.MapDelete("{id}", async (RecipeService service, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            var callerId = user.GetUserId();
            if (ownerId.HasValue && ownerId != callerId)
            {
                return Results.Forbid();
            }

            await service.DeleteRecipeAsync(id, ct);
            return Results.Ok();
        })
            .WithName("DeleteRecipe")
            .WithSummary("Delete a recipe")
            .WithDescription("Deletes a recipe and its ingredient list.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization();

        group.MapPost("{id}/clone", async (RecipeService service, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var original = await service.GetRecipeDetailAsync(id, ct);
            if (original is null)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Recipe not found for clone."
                });
            }

            var ownerId = user.GetUserId();
            var cloneDto = new CreateRecipeDto($"{original.Name} (copia)", original.Ingredients.Select(i => new RecipeIngredientDto(i.FoodId, i.QuantityGrams)).ToList());
            var (detail, error) = await service.CreateRecipeAsync(cloneDto, ownerId, ct);
            if (error is not null)
            {
                return TypedResults.Problem(new ProblemDetails { Title = "Errore clone", Status = StatusCodes.Status400BadRequest, Detail = error });
            }

            return Results.Created($"api/v1/recipes/{detail!.Id}", detail);
        })
            .WithName("CloneRecipe")
            .WithSummary("Clone a recipe")
            .WithDescription("Creates a copy of an existing recipe assigned to the current user.")
            .Produces<RecipeDetailDto>(StatusCodes.Status201Created)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapPost("{id}/recalculate", async (RecipeService service, Guid id, CancellationToken ct) =>
        {
            var (found, error) = await service.RecalculateRecipeAsync(id, ct);
            if (!found)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Recipe not found."
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
            .WithName("RecalculateRecipe")
            .WithSummary("Recalculate recipe nutrition")
            .WithDescription("Recalculates calories and nutrients for the specified recipe from its current ingredients. Clears IsNutritionStale.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound, StatusCodes.Status422UnprocessableEntity);

        group.MapPost("recalculate-stale", async (RecipeService service, CancellationToken ct) =>
        {
            var count = await service.RecalculateAllStaleRecipesAsync(ct);
            return Results.Ok(new { recalculated = count });
        })
            .WithName("RecalculateAllStaleRecipes")
            .WithSummary("Recalculate all stale recipes")
            .WithDescription("Recalculates all recipes with IsNutritionStale = true. Returns the count of updated recipes.")
            .Produces<object>(StatusCodes.Status200OK);

        group.MapPatch("{id}/quantity", async (RecipeService service, Guid id, RescaleRecipeRequest request, CancellationToken ct) =>
        {
            if (request.WeightGrams <= 0)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Peso non valido",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "Il peso della ricetta deve essere maggiore di zero."
                });
            }

            var (found, error) = await service.UpdateWeightAsync(id, request.WeightGrams, request.Recalculate, ct);
            if (!found)
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Recipe not found."
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
            .WithName("UpdateRecipeWeight")
            .WithSummary("Update recipe weight")
            .WithDescription("Updates the recipe weight. By default applies proportional rescaling (O(1), no ingredient access). Pass recalculate=true to fully recalculate nutrients from ingredients instead.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound, StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }
}
