using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class FoodEndpoints
    {
        public static IEndpointRouteBuilder MapsFoodsEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
        {
            var group = endpoints.MapGroup("api/v{version:apiVersion}/foods")
                .WithTags("Foods")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(ApiVersionFactory.Version1);

            group.MapGet("{id}", async (FoodService service, Guid id) =>
            {
                var result = await service.GetFullFood(id);
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

            group.MapGet("dashboard", async (FoodService service) =>
            {
                var result = await service.GetFoodsDashboardAsync();
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
                .WithDescription("Returns the dashboard list for foods.")
                .Produces<IList<FoodDashboardInfo>>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status404NotFound);

            group.MapGet("dashboard/{id}", async (FoodService service, Guid id) =>
            {
                var result = await service.GetFoodDashboardAsync(id);
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

            group.MapGet("getnewfood", async (FoodService service) => await service.GetFullFood(null))
                .WithName("GetNewFoodTemplate")
                .WithSummary("Get new food template")
                .WithDescription("Returns a template for creating a new food.")
                .Produces<FoodInfo>(StatusCodes.Status200OK);

            group.MapPost("Create", async (FoodService service, FoodInfo foodInfo) =>
            {
                if (await service.IsFoodNameTakenAsync(foodInfo.Name))
                {
                    return TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Nome duplicato",
                        Status = StatusCodes.Status409Conflict,
                        Detail = $"Esiste già un alimento con il nome \"{foodInfo.Name}\"."
                    });
                }

                var newFoodId = await service.InsertFullFood(foodInfo);
                return Results.Ok(newFoodId);
            })
                .WithName("CreateFood")
                .WithSummary("Create a food")
                .WithDescription("Creates a food with related nutrients and returns its identifier.")
                .Produces<Guid>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status400BadRequest)
                ;

            group.MapPut("{id}", async (FoodService service, Guid id, FoodInfo foodInfo) =>
            {
                if (await service.IsFoodNameTakenAsync(foodInfo.Name, excludeId: foodInfo.Id))
                {
                    return TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Nome duplicato",
                        Status = StatusCodes.Status409Conflict,
                        Detail = $"Esiste già un alimento con il nome \"{foodInfo.Name}\"."
                    });
                }

                var updated = await service.UpdateFullFoodAsync(foodInfo);
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
                .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound);

            group.MapDelete("{id}", async (FoodService service, Guid id) =>
            {
                await service.DeleteFoodAsync(id);
                return Results.Ok();
            })
                .WithName("DeleteFood")
                .WithSummary("Delete a food")
                .WithDescription("Deletes an existing food and its related nutrients by identifier.")
                .Produces(StatusCodes.Status200OK);

            return endpoints;
        }
    }
}
