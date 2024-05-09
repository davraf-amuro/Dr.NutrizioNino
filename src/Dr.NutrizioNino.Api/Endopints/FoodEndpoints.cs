using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class FoodEndpoints
    {
        public static void MapsFoodsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("foods");

            group.MapGet("", async (FoodsService service) => await service.GetFoodsAsync());
            group.MapGet("{id}", async (FoodsService service, Guid id) => await service.GetFoodAsync(id));
            group.MapPost("", async (FoodsService service, CreateFoodDto newFoodDto) => await service.CreateFoodAsync(newFoodDto));
            group.MapPut("{id}", async (FoodsService service, Guid id, Food food) => await service.UpdateFoodAsync(food));
            group.MapDelete("{id}", async (FoodsService service, Guid id) => await service.DeleteFoodAsync(id));

        }
    }
}
