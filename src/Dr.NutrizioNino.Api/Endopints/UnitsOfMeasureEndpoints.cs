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
                .ProducesDefaultProblem(StatusCodes.Status404NotFound);

            group.MapPost("", async (DrService service, CreateUnitOfMeasureDto newUnitOfMeasure) => await service.CreateUnitOfMeasureAsync(newUnitOfMeasure));

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
                .ProducesDefaultProblem(StatusCodes.Status404NotFound);

            group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteUnitOfMeasureAsync(id));

            return endpoints;
        }
    }
}
