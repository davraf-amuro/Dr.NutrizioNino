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

            group.MapGet("", async (FoodsService service) => await service.GetFoodsAsync())
                .WithOpenApi();
            group.MapGet("{id}", async (FoodsService service, Guid id) => await service.GetFoodAsync(id))
                .WithOpenApi();
            group.MapPost("", async (FoodsService service, CreateFoodDto newFoodDto) => await service.CreateFoodAsync(newFoodDto))
                .WithOpenApi();
            group.MapPut("{id}", async (FoodsService service, Guid id, Food food) => await service.UpdateFoodAsync(food))
                .WithOpenApi();
            group.MapDelete("{id}", async (FoodsService service, Guid id) => await service.DeleteFoodAsync(id))
                .WithOpenApi();

        }
    }
}
