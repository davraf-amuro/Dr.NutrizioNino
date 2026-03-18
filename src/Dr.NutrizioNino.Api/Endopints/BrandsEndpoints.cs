using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Mvc;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;
using TinyHelpers.AspNetCore.Extensions;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class BrandsEndpoints
    {
        public static IEndpointRouteBuilder MapsBrandsEndpoints(this IEndpointRouteBuilder endpoints, ApiVersionSet versionSet)
        {
            var group = endpoints.MapGroup("api/v{version:apiVersion}/brands")
                .WithTags("Brands")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(ApiVersionFactory.Version1);

            group.MapGet("", async (BrandService service) =>
            {
                var result = await service.GetBrandsAsync();
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
                .Produces<IList<BrandDto>>()
                .ProducesDefaultProblem(StatusCodes.Status404NotFound)
            ;
            group.MapGet("{id}", async (BrandService service, Guid id) =>
            {
                var result = await service.GetBrandAsync(id);
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
                .ProducesDefaultProblem(StatusCodes.Status404NotFound)
                ;
            group.MapPost("", async (BrandService service, CreateBrandDto newBrand) =>
            {
                if (await service.IsBrandNameTakenAsync(newBrand.Name))
                {
                    return TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Nome duplicato",
                        Status = StatusCodes.Status409Conflict,
                        Detail = $"Esiste già una marca con il nome \"{newBrand.Name}\"."
                    });
                }

                var result = await service.CreateBrandAsync(newBrand);
                return Results.Ok(result);
            })
                .WithName("CreateBrand")
                .WithSummary("Create a new brand")
                .WithDescription("Creates a new brand and returns the created entity.")
                .Produces<BrandDto>(StatusCodes.Status200OK)
                .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status409Conflict)
                ;
            group.MapPut("{id}", async (BrandService service, Guid id, Brand brand) =>
            {
                if (await service.IsBrandNameTakenAsync(brand.Name, excludeId: brand.Id))
                {
                    return TypedResults.Problem(new ProblemDetails
                    {
                        Title = "Nome duplicato",
                        Status = StatusCodes.Status409Conflict,
                        Detail = $"Esiste già una marca con il nome \"{brand.Name}\"."
                    });
                }

                var updated = await service.UpdateBrandAsync(brand);
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
                .ProducesDefaultProblem(StatusCodes.Status400BadRequest, StatusCodes.Status404NotFound);
            group.MapGet("{id}/is-in-use", async (BrandService service, Guid id) =>
            {
                var inUse = await service.IsBrandInUseAsync(id);
                return Results.Ok(inUse);
            })
                .WithName("IsBrandInUse")
                .WithSummary("Check if brand is in use")
                .WithDescription("Returns true if the brand is referenced by one or more foods.")
                .Produces<bool>(StatusCodes.Status200OK);

            group.MapDelete("{id}", async (BrandService service, Guid id) =>
            {
                var deleted = await service.DeleteBrandAsync(id);
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
                .ProducesDefaultProblem(StatusCodes.Status404NotFound)
                ;

            return endpoints;
        }
    }
}
