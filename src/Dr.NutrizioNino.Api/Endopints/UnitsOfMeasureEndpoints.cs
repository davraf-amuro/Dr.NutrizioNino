using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class UnitsOfMeasureEndpoints
    {
        public static void MapUnitsOfMeasureEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("unitsOfMeasures")
                .WithOpenApi()
                .WithTags("Units Of Measures");

            group.MapGet("", async (UnitsOfMeasuresService service) => await service.GetUnitsOfMeasuresAsync())
                .WithOpenApi();
            group.MapGet("{id}", async (UnitsOfMeasuresService service, Guid id) => await service.GetUnitOfMeasureAsync(id))
                .WithOpenApi();
            group.MapPost("", async (UnitsOfMeasuresService service, CreateUnitOfMeasureDto newUnitOfMeasure) => await service.CreateUnitOfMeasureAsync(newUnitOfMeasure))
                .WithOpenApi();
            group.MapPut("{id}", async (UnitsOfMeasuresService service, Guid id, UnitOfMeasure unitOfMeasure) => await service.UpdateUnitOfMeasureAsync(unitOfMeasure))
                .WithOpenApi();
            group.MapDelete("{id}", async (UnitsOfMeasuresService service, Guid id) => await service.DeleteUnitOfMeasureAsync(id))
                .WithOpenApi();
        }
    }
}
