using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Helpers;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endopints;

public static class BrandsEndpoints
{
    private record BrandListItem(Guid Id, string Name, bool IsOwner);

    public static IEndpointRouteBuilder MapsBrandsEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
    {
        var group = endpoints.MapGroup("api/v{version:apiVersion}/brands")
            .WithTags("Brands")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(ApiVersionFactory.Version1);

        group.MapGet("", async (BrandService service, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = user.GetUserId();
            var raw = await service.GetBrandsAsync(b => new { b.Id, b.Name, b.OwnerId }, ct);
            var result = raw.Select(b => new BrandListItem(b.Id, b.Name, userId.HasValue && b.OwnerId.HasValue && b.OwnerId == userId)).ToList();
            return result.Count > 0
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "No brands found."
                });
        })
            .WithName("GetBrands")
            .WithSummary("Get all brands")
            .WithDescription("Returns all available brands.")
            .Produces<IList<BrandListItem>>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapGet("{id}", async (BrandService service, Guid id, CancellationToken ct) =>
        {
            var result = await service.GetBrandAsync(id, ct);
            return result is not null
                ? Results.Ok(result)
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Brand not found."
                });
        })
            .WithName("GetBrandById")
            .WithSummary("Get brand by id")
            .WithDescription("Returns a brand for the provided identifier.")
            .Produces<BrandDto>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound);

        group.MapPost("", async (BrandService service, CreateBrandDto newBrand, ClaimsPrincipal user, CancellationToken ct) =>
        {
            if (await service.IsBrandNameTakenAsync(newBrand.Name, ct: ct))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già una marca con il nome \"{newBrand.Name}\"."
                });
            }

            var ownerId = user.GetUserId();
            var result = await service.CreateBrandAsync(newBrand, ownerId, ct);
            return Results.Ok(new BrandListItem(result.Id, result.Name, IsOwner: true));
        })
            .WithName("CreateBrand")
            .WithSummary("Create a new brand")
            .WithDescription("Creates a new brand and returns the created entity.")
            .Produces<BrandListItem>(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict)
            .RequireAuthorization();

        group.MapPut("{id}", async (BrandService service, Guid id, Brand brand, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            var callerId = user.GetUserId();
            if (ownerId.HasValue && ownerId != callerId)
                return Results.Forbid();

            if (await service.IsBrandNameTakenAsync(brand.Name, excludeId: brand.Id, ct: ct))
            {
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Nome duplicato",
                    Status = StatusCodes.Status409Conflict,
                    Detail = $"Esiste già una marca con il nome \"{brand.Name}\"."
                });
            }

            var updated = await service.UpdateBrandAsync(brand, ct);
            return updated
                ? Results.Ok()
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Brand not found for update."
                });
        })
            .AddEndpointFilter(async (context, next) =>
            {
                var routeId = context.GetArgument<Guid>(1);
                var brand = context.GetArgument<Brand>(2);
                if (brand.Id == Guid.Empty)
                {
                    brand.Id = routeId;
                }

                if (brand.Id != routeId)
                {
                    return TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Invalid Request",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Brand id in route and body must match."
                    });
                }

                return await next(context);
            })
            .WithName("UpdateBrand")
            .WithSummary("Update an existing brand")
            .WithDescription("Updates an existing brand by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status403Forbidden, StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapGet("{id}/is-in-use", async (BrandService service, Guid id, CancellationToken ct) =>
        {
            var inUse = await service.IsBrandInUseAsync(id, ct);
            return Results.Ok(inUse);
        })
            .WithName("IsBrandInUse")
            .WithSummary("Check if brand is in use")
            .WithDescription("Returns true if the brand is referenced by one or more foods.")
            .Produces<bool>(StatusCodes.Status200OK);

        group.MapDelete("{id}", async (BrandService service, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var ownerId = await service.GetOwnerIdAsync(id, ct);
            var callerId = user.GetUserId();
            if (ownerId.HasValue && ownerId != callerId)
                return Results.Forbid();

            var deleted = await service.DeleteBrandAsync(id, ct);
            return deleted
                ? Results.Ok()
                : TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Brand not found for delete."
                });
        })
            .WithName("DeleteBrand")
            .WithSummary("Delete a brand")
            .WithDescription("Deletes an existing brand by identifier.")
            .Produces(StatusCodes.Status200OK)
            .ProducesDefaultProblem(StatusCodes.Status403Forbidden, StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapPost("{id}/clone", async (BrandService service, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var original = await service.GetBrandAsync(id, ct);
            if (original is null)
                return TypedResults.Problem(new ProblemDetails
                {
                    Title = "Data Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = "Brand not found for clone."
                });

            var ownerId = user.GetUserId();
            var cloned = await service.CreateBrandAsync(new CreateBrandDto($"{original.Name} (copia)"), ownerId, ct);
            return Results.Created($"api/v1/brands/{cloned.Id}", cloned);
        })
            .WithName("CloneBrand")
            .WithSummary("Clone a brand")
            .WithDescription("Creates a copy of an existing brand assigned to the current user.")
            .Produces<BrandDto>(StatusCodes.Status201Created)
            .ProducesDefaultProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return endpoints;
    }
}
