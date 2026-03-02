using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class UnitsOfMeasureEndpoints
    {
        public static IEndpointRouteBuilder MapUnitsOfMeasureEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
        {
            var group = endpoints.MapGroup("api/v{version:apiVersion}/unitsOfMeasures")
                .WithTags("Units Of Measures")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(ApiVersionFactory.Version1);

            group.MapGet("", async (DrService service) =>
            {
                var result = await service.GetUnitsOfMeasuresAsync();
                return result.Count > 0
                    ? Results.Ok(result)
                    : TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Data Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = "No units of measure found."
                    });
            })
                .WithName("GetUnitsOfMeasure")
                .WithSummary("Get all units of measure")
                .WithDescription("Returns all available units of measure.")
                .Produces<IList<UnitOfMeasureDto>>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status404NotFound);

            group.MapGet("{id}", async (DrService service, Guid id) =>
            {
                var result = await service.GetUnitOfMeasureAsync(id);
                return result is not null
                    ? Results.Ok(result)
                    : TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Data Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = "Unit of measure not found."
                    });
            })
                .WithName("GetUnitOfMeasureById")
                .WithSummary("Get unit of measure by id")
                .WithDescription("Returns a unit of measure for the specified identifier.")
                .Produces<UnitOfMeasure>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status404NotFound);

            group.MapPost("", async (DrService service, CreateUnitOfMeasureDto newUnitOfMeasure) => await service.CreateUnitOfMeasureAsync(newUnitOfMeasure))
                .WithName("CreateUnitOfMeasure")
                .WithSummary("Create unit of measure")
                .WithDescription("Creates a unit of measure and returns the created entity.")
                .Produces<UnitOfMeasure>(StatusCodes.Status200OK);

            group.MapPut("{id}", async (DrService service, Guid id, UnitOfMeasure unitOfMeasure) =>
            {
                var result = await service.UpdateUnitOfMeasureAsync(unitOfMeasure);
                return result is not null
                    ? Results.Ok(result)
                    : TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Data Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = "Unit of measure not found for update."
                    });
            })
                .WithName("UpdateUnitOfMeasure")
                .WithSummary("Update unit of measure")
                .WithDescription("Updates an existing unit of measure by identifier.")
                .Produces<UnitOfMeasureDto>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status404NotFound);

            group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteUnitOfMeasureAsync(id))
                .WithName("DeleteUnitOfMeasure")
                .WithSummary("Delete unit of measure")
                .WithDescription("Deletes a unit of measure by identifier.")
                .Produces(StatusCodes.Status200OK);

            return endpoints;
        }
    }
}
