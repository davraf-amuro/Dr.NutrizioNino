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

            group.MapGet("", async (DrService service) =>
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
            group.MapGet("{id}", async (DrService service, Guid id) =>
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
            group.MapPost("", async (DrService service, CreateBrandDto newBrand) => await service.CreateBrandAsync(newBrand))
                .WithName("CreateBrand")
                .WithSummary("Create a new brand")
                .WithDescription("Creates a new brand and returns the created entity.")
                .Produces<BrandDto>(StatusCodes.Status200OK)
                ;
            group.MapPut("{id}", async (DrService service, Guid id, Brand brand) => await service.UpdateBrandAsync(brand))
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
                .ProducesDefaultProblem(StatusCodes.Status400BadRequest);
            group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteBrandAsync(id))
                .WithName("DeleteBrand")
                .WithSummary("Delete a brand")
                .WithDescription("Deletes an existing brand by identifier.")
                .Produces(StatusCodes.Status200OK)
                ;

            return endpoints;
        }
    }
}
