using Dr.NutrizioNino.Api.Dto;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class BrandsEndpoints
    {
        public static void MapsBrandsEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("brands");

            group.MapGet("", async (BrandsService service) => {
                var result = await service.GetBrandsAsync();
                return result.Success ? Results.Ok(result) : Results.NotFound(result);
            }).Produces<ApiResponseDto<BrandDto>>()
            ;
            group.MapGet("{id}", async (BrandsService service, Guid id) => await service.GetBrandAsync(id));
            group.MapPost("", async (BrandsService service, CreateBrandDto newBrand) => await service.CreateBrandAsync(newBrand));
            group.MapPut("{id}", async (BrandsService service, Guid id, Brand brand) => await service.UpdateBrandAsync(brand));
            group.MapDelete("{id}", async (BrandsService service, Guid id) => await service.DeleteBrandAsync(id));

        }
    }
}
