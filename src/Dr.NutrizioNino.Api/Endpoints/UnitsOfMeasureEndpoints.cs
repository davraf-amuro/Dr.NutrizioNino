using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endpoints;

public static class UnitsOfMeasureEndpoints
{
    public static IEndpointRouteBuilder MapUnitsOfMeasureEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/unitsOfMeasures")
            .WithTags("Units Of Measures")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1);

        group.MapGet("", async (UnitsOfMeasureService service, CancellationToken ct) =>
        {
            var result = await service.GetUnitsOfMeasuresAsync(ct);
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

        group.MapGet("{id}", async (UnitsOfMeasureService service, Guid id, CancellationToken ct) =>
        {
            var result = await service.GetUnitOfMeasureAsync(id, ct);
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
            .Produces<UnitOfMeasureDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapPost("", async (UnitsOfMeasureService service, CreateUnitOfMeasureDto newUnitOfMeasure, CancellationToken ct) =>
        {
            var (result, entity) = await service.CreateUnitOfMeasureAsync(newUnitOfMeasure, ct);
            return result switch
            {
                UomOperationResult.Success => Results.Ok(entity),
                UomOperationResult.DuplicateName => TypedResults.Problem(new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Esiste già un'unità di misura con questo nome."
                }),
                _ => TypedResults.Problem(new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Esiste già un'unità di misura con questa abbreviazione."
                })
            };
        })
            .WithName("CreateUnitOfMeasure")
            .WithSummary("Create unit of measure")
            .WithDescription("Creates a unit of measure and returns the created entity.")
            .Produces<UnitOfMeasureDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict);

        group.MapPut("{id}", async (UnitsOfMeasureService service, Guid id, UnitOfMeasure unitOfMeasure, CancellationToken ct) =>
        {
            var result = await service.UpdateUnitOfMeasureAsync(unitOfMeasure, ct);
            return result switch
            {
                UomOperationResult.Success => Results.Ok(),
                UomOperationResult.DuplicateName => TypedResults.Problem(new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Esiste già un'unità di misura con questo nome."
                }),
                UomOperationResult.DuplicateAbbreviation => TypedResults.Problem(new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "Esiste già un'unità di misura con questa abbreviazione."
                }),
                _ => TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Unit of measure not found for update."
                })
            };
        })
            .AddEndpointFilter(async (context, next) =>
            {
                var routeId = context.GetArgument<Guid>(1);
                var unitOfMeasure = context.GetArgument<UnitOfMeasure>(2);
                if (unitOfMeasure.Id == Guid.Empty)
                {
                    unitOfMeasure.Id = routeId;
                }

                if (unitOfMeasure.Id != routeId)
                {
                    return TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "UnitOfMeasure id in route and body must match."
                    });
                }

                return await next(context);
            })
            .WithName("UpdateUnitOfMeasure")
            .WithSummary("Update unit of measure")
            .WithDescription("Updates an existing unit of measure by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound, StatusCodes.Status409Conflict);

        group.MapDelete("{id}", async (UnitsOfMeasureService service, Guid id, CancellationToken ct) =>
        {
            var result = await service.DeleteUnitOfMeasureAsync(id, ct);
            return result switch
            {
                UomOperationResult.Success => Results.Ok(),
                UomOperationResult.InUse => TypedResults.Problem(new ProblemDetails
                {
                    Title = "Conflict",
                    Status = StatusCodes.Status409Conflict,
                    Detail = "L'unità di misura è in uso e non può essere eliminata."
                }),
                _ => TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Unit of measure not found for delete."
                })
            };
        })
            .WithName("DeleteUnitOfMeasure")
            .WithSummary("Delete unit of measure")
            .WithDescription("Deletes a unit of measure by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict);

        return endpoints;
    }
}
