using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;

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

            //group.MapPost("", async (DrService service, CreateFoodDto newFoodDto) => await service.CreateFoodAsync(newFoodDto))
            //    .WithOpenApi();
            //group.MapPut("{id}", async (DrService service, Guid id, Food food) => await service.UpdateFoodAsync(food))
            //    .WithOpenApi();
            //group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteFoodAsync(id))
            //    .WithOpenApi();

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
