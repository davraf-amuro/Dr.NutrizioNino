using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endopints;

public static class FoodEndpoints
{
    public static IEndpointRouteBuilder MapsFoodsEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/foods")
            .WithTags("Foods")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1);

        group.MapGet("{id}", async (FoodService service, Guid id, CancellationToken ct) =>
        {
            var result = await service.GetFullFood(id, ct);
            return result is not null
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Food not found."
                });
        })
            .WithName("GetFoodById")
            .WithSummary("Get food details")
            .WithDescription("Returns complete food details for the specified identifier.")
            .Produces<FoodInfo>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapGet("dashboard", async (FoodService service, string? name, CancellationToken ct) =>
        {
            var result = await service.GetFoodsDashboardAsync(name, ct);
            return result.Count > 0
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "No dashboard data found."
                });
        })
            .WithName("GetFoodsDashboard")
            .WithSummary("Get foods dashboard")
            .WithDescription("Returns the dashboard list for foods. Optional query param: name (partial match, case-insensitive).")
            .Produces<IList<FoodDashboardInfo>>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapGet("dashboard/{id}", async (FoodService service, Guid id, CancellationToken ct) =>
        {
            var result = await service.GetFoodDashboardAsync(id, ct);
            return result is not null
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Dashboard item not found."
                });
        })
            .WithName("GetFoodDashboardById")
            .WithSummary("Get food dashboard item")
            .WithDescription("Returns a dashboard item for the specified food identifier.")
            .Produces<FoodDashboardInfo>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapGet("newgui", () => Guid.NewGuid().ToString())
            .WithName("GetNewGuiToken")
            .WithSummary("Generate new gui token")
            .WithDescription("Returns a new guid string for GUI initialization.")
            .Produces<string>(StatusCodes.Status200OK);

        group.MapGet("getnewfood", async (FoodService service, CancellationToken ct) =>
            await service.GetFullFood(null, ct))
            .WithName("GetNewFoodTemplate")
            .WithSummary("Get new food template")
            .WithDescription("Returns a template for creating a new food.")
            .Produces<FoodInfo>(StatusCodes.Status200OK);

        group.MapPost("Create", async (FoodService service, FoodInfo foodInfo, ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (await service.IsFoodNameTakenAsync(foodInfo.Name, ct: ct))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già un alimento con il nome \"{foodInfo.Name}\"."
                });
            }

            var ownerId = user.GetUserId();
            var newFoodId = await service.InsertFullFood(foodInfo, ownerId, ct);
            return Results.Ok(foodInfo with { Id = newFoodId });
        })
            .WithName("CreateFood")
            .WithSummary("Create a food")
            .WithDescription("Creates a food with related nutrients and returns the created food with its assigned identifier.")
            .Produces<FoodInfo>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict)
            .RequireAuthorization();

        group.MapPut("{id}", async (FoodService service, Guid id, FoodInfo foodInfo, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            var callerId = user.GetUserId();
            if (ownerId.HasValue && ownerId != callerId)
                return Results.Forbid();

            if (await service.IsFoodNameTakenAsync(foodInfo.Name, excludeId: foodInfo.Id, ct: ct))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già un alimento con il nome \"{foodInfo.Name}\"."
                });
            }

            var updated = await service.UpdateFullFoodAsync(foodInfo, ct);
            return updated
                ? Results.Ok()
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Food not found for update."
                });
        })
            .AddEndpointFilter(async (context, next) =>
            {
                var routeId = context.GetArgument<Guid>(1);
                var foodInfo = context.GetArgument<FoodInfo>(2);

                if (foodInfo.Id != routeId)
                {
                    return TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Food id in route and body must match."
                    });
                }

                return await next(context);
            })
            .WithName("UpdateFood")
            .WithSummary("Update a food")
            .WithDescription("Updates an existing food with all related nutrients by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status403Forbidden, StatusCodes.Status404NotFound, StatusCodes.Status409Conflict)
            .RequireAuthorization();

        group.MapDelete("{id}", async (FoodService service, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            var callerId = user.GetUserId();
            if (ownerId.HasValue && ownerId != callerId)
                return Results.Forbid();

            await service.DeleteFoodAsync(id, ct);
            return Results.Ok();
        })
            .WithName("DeleteFood")
            .WithSummary("Delete a food")
            .WithDescription("Deletes an existing food and its related nutrients by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization();

        group.MapPost("{id}/clone", async (FoodService service, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var original = await service.GetFullFood(id, ct);
            if (original is null)
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Food not found for clone."
                });

            var ownerId = user.GetUserId();
            var clonedId = await service.InsertFullFood(original with { Id = Guid.Empty, Name = $"{original.Name} (copia)" }, ownerId, ct);
            return Results.Created($"api/v1/foods/{clonedId}", original with { Id = clonedId });
        })
            .WithName("CloneFood")
            .WithSummary("Clone a food")
            .WithDescription("Creates a copy of an existing food assigned to the current user.")
            .Produces<FoodInfo>(StatusCodes.Status201Created)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return endpoints;
    }
}
