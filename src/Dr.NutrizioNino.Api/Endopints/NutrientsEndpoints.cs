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
                .WithName("GetNutrients")
                .WithSummary("Get all nutrients")
                .WithDescription("Returns all available nutrients ordered by display position.")
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
                .WithName("GetNutrientById")
                .WithSummary("Get nutrient by id")
                .WithDescription("Returns the nutrient associated with the specified identifier.")
                .Produces<Nutrient>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status404NotFound)
                ;
            group.MapPost("", async (DrService service, CreateNutrientDto newNutrient) => await service.CreateNutrientAsync(newNutrient))
                .WithName("CreateNutrient")
                .WithSummary("Create nutrient")
                .WithDescription("Creates a nutrient and returns the created entity.")
                .Produces<Nutrient>(StatusCodes.Status200OK)
                ;
            group.MapPut("{id}", async (DrService service, Guid id, Nutrient nutrient) => await service.UpdateNutrientAsync(nutrient))
                .WithName("UpdateNutrient")
                .WithSummary("Update nutrient")
                .WithDescription("Updates an existing nutrient by identifier.")
                .Produces(StatusCodes.Status200OK)
                ;
            group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteNutrientAsync(id))
                .WithName("DeleteNutrient")
                .WithSummary("Delete nutrient")
                .WithDescription("Deletes a nutrient by identifier.")
                .Produces(StatusCodes.Status200OK)
                ;

            return endpoints;
        }
    }
}
