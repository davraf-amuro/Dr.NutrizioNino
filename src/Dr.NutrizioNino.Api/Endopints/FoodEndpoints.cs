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

            group.MapGet("", async (DrService service) => await service.GetFoodsAsync())
                .WithOpenApi();
            group.MapGet("{id}", async (DrService service, Guid id) => await service.GetFoodAsync(id))
                .WithOpenApi();
            group.MapPost("", async (DrService service, CreateFoodDto newFoodDto) => await service.CreateFoodAsync(newFoodDto))
                .WithOpenApi();
            group.MapPut("{id}", async (DrService service, Guid id, Food food) => await service.UpdateFoodAsync(food))
                .WithOpenApi();
            group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteFoodAsync(id))
                .WithOpenApi();

            group.MapGet("dashboard", async (DrService service) => await service.GetFoodsDashboardAsync())
                .WithOpenApi();

            group.MapGet("newgui", () => Guid.NewGuid().ToString()).WithOpenApi().WithName("newgui");

            group.MapGet("factorygetnew", async (DrService service) => await service.FactoryGetNew())
                .WithOpenApi()
                .WithName("factorygetnew")
                .Produces<ApiResponseDto<FoodCreationTemplateDto>>(StatusCodes.Status200OK);

        }
    }
}
