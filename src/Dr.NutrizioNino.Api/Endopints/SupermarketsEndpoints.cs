using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endopints;

public static class SupermarketsEndpoints
{
    public static IEndpointRouteBuilder MapsSupermarketsEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/supermarkets")
            .WithTags("Supermarkets")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1);

        group.MapGet("", async (SupermarketService service, CancellationToken ct) =>
        {
            var result = await service.GetSupermarketsAsync(ct);
            return result.Count > 0
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "No supermarkets found."
                });
        })
            .WithName("GetSupermarkets")
            .WithSummary("Get all supermarkets")
            .WithDescription("Returns all available supermarkets.")
            .Produces<IList<SupermarketDto>>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapGet("{id}", async (SupermarketService service, Guid id, CancellationToken ct) =>
        {
            var result = await service.GetSupermarketAsync(id, ct);
            return result is not null
                ? Results.Ok(new SupermarketDto(result.Id, result.Name))
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Supermarket not found."
                });
        })
            .WithName("GetSupermarketById")
            .WithSummary("Get supermarket by id")
            .WithDescription("Returns a supermarket for the provided identifier.")
            .Produces<SupermarketDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapPost("", async (SupermarketService service, CreateSupermarketDto dto, CancellationToken ct) =>
        {
            if (await service.IsSupermarketNameTakenAsync(dto.Name, ct: ct))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già un supermercato con il nome \"{dto.Name}\"."
                });
            }

            var result = await service.CreateSupermarketAsync(dto, ct);
            return Results.Ok(new SupermarketDto(result.Id, result.Name));
        })
            .WithName("CreateSupermarket")
            .WithSummary("Create a new supermarket")
            .WithDescription("Creates a new supermarket and returns the created entity.")
            .Produces<SupermarketDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict);

        group.MapPut("{id}", async (SupermarketService service, Guid id, Supermarket supermarket, CancellationToken ct) =>
        {
            if (await service.IsSupermarketNameTakenAsync(supermarket.Name, excludeId: supermarket.Id, ct: ct))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già un supermercato con il nome \"{supermarket.Name}\"."
                });
            }

            var updated = await service.UpdateSupermarketAsync(supermarket, ct);
            return updated
                ? Results.Ok()
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Supermarket not found for update."
                });
        })
            .AddEndpointFilter(async (context, next) =>
            {
                var routeId = context.GetArgument<Guid>(1);
                var supermarket = context.GetArgument<Supermarket>(2);
                if (supermarket.Id == Guid.Empty)
                {
                    supermarket.Id = routeId;
                }

                if (supermarket.Id != routeId)
                {
                    return TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Supermarket id in route and body must match."
                    });
                }

                return await next(context);
            })
            .WithName("UpdateSupermarket")
            .WithSummary("Update an existing supermarket")
            .WithDescription("Updates an existing supermarket by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound, StatusCodes.Status409Conflict);

        group.MapGet("{id}/is-in-use", async (SupermarketService service, Guid id, CancellationToken ct) =>
            Results.Ok(await service.IsSupermarketInUseAsync(id, ct)))
            .WithName("IsSupermarketInUse")
            .WithSummary("Check if supermarket is in use")
            .WithDescription("Returns true if the supermarket is referenced by one or more foods.")
            .Produces<bool>(StatusCodes.Status200OK);

        group.MapDelete("{id}", async (SupermarketService service, Guid id, CancellationToken ct) =>
        {
            var deleted = await service.DeleteSupermarketAsync(id, ct);
            return deleted
                ? Results.Ok()
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Supermarket not found for delete."
                });
        })
            .WithName("DeleteSupermarket")
            .WithSummary("Delete a supermarket")
            .WithDescription("Deletes an existing supermarket by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        return endpoints;
    }
}
