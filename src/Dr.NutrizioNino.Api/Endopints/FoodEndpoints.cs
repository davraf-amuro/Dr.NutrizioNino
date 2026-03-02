using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Services;

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
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
                .Produces<FoodInfo>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("dashboard", async (DrService service) =>
            {
                var result = await service.GetFoodsDashboardAsync();
                return result.Count > 0 ? Results.Ok(result) : Results.NotFound();
            })
                .WithName("dashboard")
                .Produces<IList<FoodDashboardInfo>>(StatusCodes.Status200OK);

            group.MapGet("dashboard/{id}", async (DrService service, Guid id) =>
            {
                var result = await service.GetFoodDashboardAsync(id);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            })
                .WithName("dashboardrow")
                .Produces<FoodDashboardInfo>(StatusCodes.Status200OK);

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
