using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Services;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class FoodEndpoints
    {
        public static void MapsFoodsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("foods")
                .WithOpenApi()
                .WithTags("Foods");

            group.MapGet("{id}", async (DrService service, Guid id) => await service.GetFullFood(id))
                .WithOpenApi();

            group.MapGet("dashboard", async (DrService service) => await service.GetFoodsDashboardAsync())
                .WithOpenApi()
                .WithName("dashboard")
                .Produces<ApiResponseMultipleDto<FoodDashboardInfo>>(StatusCodes.Status200OK);

            group.MapGet("dashboard/{id}", async (DrService service, Guid id) => await service.GetFoodDashboardAsync(id))
                .WithOpenApi()
                .WithName("dashboardrow")
                .Produces<ApiResponseMultipleDto<FoodDashboardInfo>>(StatusCodes.Status200OK);

            group.MapGet("newgui", () => Guid.NewGuid().ToString()).WithOpenApi().WithName("newgui");

            group.MapGet("getnewfood", async (DrService service) => await service.GetFullFood(null))
                .WithOpenApi()
                .WithName("getnewfood")
                .Produces<ApiResponseSingleDto<FoodInfo>>(StatusCodes.Status200OK);

        }
    }
}
