using Asp.Versioning;
using Asp.Versioning.Builder;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;

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
                return result.Count > 0 ? Results.Ok(result) : Results.NotFound();
            })
                .Produces<IList<UnitOfMeasureDto>>(StatusCodes.Status200OK);

            group.MapGet("{id}", async (DrService service, Guid id) => await service.GetUnitOfMeasureAsync(id));

            group.MapPost("", async (DrService service, CreateUnitOfMeasureDto newUnitOfMeasure) => await service.CreateUnitOfMeasureAsync(newUnitOfMeasure));

            group.MapPut("{id}", async (DrService service, Guid id, UnitOfMeasure unitOfMeasure) =>
            {
                var result = await service.UpdateUnitOfMeasureAsync(unitOfMeasure);
                return result is not null ? Results.Ok(result) : Results.NotFound();
            });

            group.MapDelete("{id}", async (DrService service, Guid id) => await service.DeleteUnitOfMeasureAsync(id));

            return endpoints;
        }
    }
}
