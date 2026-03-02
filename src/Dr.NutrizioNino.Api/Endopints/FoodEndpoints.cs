using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Infrastructure.Models;
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

            group.MapGet("{id}", async (DrService service, Guid id) =>
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
                .Produces<FoodInfo>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status404NotFound);

            group.MapGet("dashboard", async (DrService service) =>
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
                .WithName("dashboard")
                .Produces<IList<FoodDashboardInfo>>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status404NotFound);

            group.MapGet("dashboard/{id}", async (DrService service, Guid id) =>
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
                .WithName("dashboardrow")
                .Produces<FoodDashboardInfo>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status404NotFound);

            group.MapGet("newgui", () => Guid.NewGuid().ToString()).WithName("newgui");

            group.MapGet("getnewfood", async (DrService service) => await service.GetFullFood(null))
                .WithName("getnewfood")
                .Produces<FoodInfo>(StatusCodes.Status200OK);

            group.MapPost("Create", async (DrService service, FoodInfo foodInfo) =>
            {
                var newFoodId = await service.InsertFullFood(foodInfo);
                return Results.Ok(newFoodId);
            })
                ;

            return endpoints;
        }
    }
}
