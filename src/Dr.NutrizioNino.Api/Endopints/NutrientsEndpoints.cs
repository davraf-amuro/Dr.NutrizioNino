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

            group.MapGet("", async (NutrientService service) =>
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
            group.MapGet("{id}", async (NutrientService service, Guid id) =>
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
            group.MapPost("", async (NutrientService service, CreateNutrientDto newNutrient) =>
            {
                var result = await service.CreateNutrientAsync(newNutrient);
                return result is not null
                    ? Results.Ok(result)
                    : TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Conflict",
                        Status = StatusCodes.Status409Conflict,
                        Detail = "Esiste già un nutriente con questo nome."
                    });
            })
                .WithName("CreateNutrient")
                .WithSummary("Create nutrient")
                .WithDescription("Creates a nutrient and returns the created entity.")
                .Produces<Nutrient>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict)
                ;
            group.MapPut("{id}", async (NutrientService service, Guid id, Nutrient nutrient) =>
            {
                var result = await service.UpdateNutrientAsync(nutrient);
                return result switch
                {
                    NutrientOperationResult.Success => Results.Ok(),
                    NutrientOperationResult.Conflict => TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Conflict",
                        Status = StatusCodes.Status409Conflict,
                        Detail = "Esiste già un nutriente con questo nome."
                    }),
                    _ => TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Data Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = "Nutrient not found for update."
                    })
                };
            })
                .AddEndpointFilter(async (context, next) =>
                {
                    var routeId = context.GetArgument<Guid>(1);
                    var nutrient = context.GetArgument<Nutrient>(2);
                    if (nutrient.Id == Guid.Empty)
                    {
                        nutrient.Id = routeId;
                    }

                    if (nutrient.Id != routeId)
                    {
                        return TypedResults.Problem(new ProblemDetails
                        {
                            Title = "Invalid Request",
                            Status = StatusCodes.Status400BadRequest,
                            Detail = "Nutrient id in route and body must match."
                        });
                    }

                    return await next(context);
                })
                .WithName("UpdateNutrient")
                .WithSummary("Update nutrient")
                .WithDescription("Updates an existing nutrient by identifier.")
                .Produces(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound, StatusCodes.Status409Conflict)
                ;
            group.MapDelete("{id}", async (NutrientService service, Guid id) =>
            {
                var result = await service.DeleteNutrientAsync(id);
                return result switch
                {
                    NutrientOperationResult.Success => Results.Ok(),
                    NutrientOperationResult.Conflict => TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Conflict",
                        Status = StatusCodes.Status409Conflict,
                        Detail = "Il nutriente è in uso e non può essere eliminato."
                    }),
                    _ => TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Data Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = "Nutrient not found for delete."
                    })
                };
            })
                .WithName("DeleteNutrient")
                .WithSummary("Delete nutrient")
                .WithDescription("Deletes a nutrient by identifier.")
                .Produces(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict)
                ;

            return endpoints;
        }
    }
}
