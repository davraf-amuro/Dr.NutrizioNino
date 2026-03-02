using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class NutrientsEndpoints
    {
        public static IEndpointRouteBuilder MapsNutrientsEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
        {
            var group = endpoints.MapGroup("api/v{version:apiVersion}/nutrients")
                .WithTags("Nutrients")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(ApiVersionFactory.Version1);

            group.MapGet("", async (DrService service) =>
            {
                var result = await service.GetNutrientsAsync();
                return result.Count > 0
                    ? Results.Ok(result)
                    : TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Data Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = "No nutrients found."
                    });
            })
                .Produces<IList<NutrientInfo>>()
                .ProducesDefaultProblem(StatusCodes.Status404NotFound)
            ;
            group.MapGet("{id}", async (DrService service, Guid id) =>
            {
                var result = await service.GetNutrientAsync(id);
                return result is not null
                    ? Results.Ok(result)
                    : TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Data Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = "Nutrient not found."
                    });
            })
                .ProducesDefaultProblem(StatusCodes.Status404NotFound)
                ;
            group.MapPost("", async (DrService service, CreateNutrientDto newNutrient) => await service.CreateNutrientAsync(newNutrient))
                ;
            group.MapPut("{id}", async (DrService service, Guid id, Nutrient nutrient) => await service.UpdateNutrientAsync(nutrient))
                ;
            group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteNutrientAsync(id))
                ;

            return endpoints;
        }
    }
}
