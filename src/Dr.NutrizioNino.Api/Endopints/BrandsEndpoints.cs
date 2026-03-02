using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;

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
                return result.Count > 0 ? Results.Ok(result) : Results.NotFound();
            })
                .Produces<IList<BrandDto>>()
            ;
            group.MapGet("{id}", async (DrService service, Guid id) => await service.GetBrandAsync(id))
                ;
            group.MapPost("", async (DrService service, CreateBrandDto newBrand) => await service.CreateBrandAsync(newBrand))
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
                        return Results.BadRequest("Brand id in route and body must match.");
                    }

                    return await next(context);
                });
            group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteBrandAsync(id))
                ;

            return endpoints;
        }
    }
}
