using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class BrandsEndpoints
    {
        public static void MapsBrandsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("brands").WithTags("Brands");

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
                ;
            group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteBrandAsync(id))
                ;

        }
    }
}
