using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class NutrientsEndpoints
    {
        public static void MapsNutrientsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("nutrients").WithOpenApi().WithTags("Nutrients");

            group.MapGet("", async (NutrientsService service) =>
            {
                var result = await service.GetNutrientsAsync();
                return result.Success ? Results.Ok(result) : Results.NotFound(result);
            })
                .WithOpenApi()
                .Produces<ApiResponseDto<NutrientDto>>()
            ;
            group.MapGet("{id}", async (NutrientsService service, Guid id) => await service.GetNutrientAsync(id))
                .WithOpenApi();
            group.MapPost("", async (NutrientsService service, CreateNutrientDto newNutrient) => await service.CreateNutrientAsync(newNutrient))
                .WithOpenApi();
            group.MapPut("{id}", async (NutrientsService service, Guid id, Nutrient nutrient) => await service.UpdateNutrientAsync(nutrient))
                .WithOpenApi();
            group.MapDelete("{id}", async (NutrientsService service, Guid id) => await service.DeleteBrandAsync(id))
                .WithOpenApi();

            group.MapGet("getforfoodcreating", async (NutrientsService service) =>
            {
                var result = await service.GetNutrientsForFoodCreating();
                return result.Success ? Results.Ok(result) : Results.NotFound(result);
            }) 
                .WithOpenApi()
                .Produces<ApiResponseDto<NutrientsGetForFoodCreatingInfo>>();

        }
    }
}
