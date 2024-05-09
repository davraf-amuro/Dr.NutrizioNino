using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Api.Services;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Endopints
{
    public static class UnitsOfMeasureEndpoints
    {
        public static void MapUnitsOfMeasureEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("unitsOfMeasure");

            group.MapGet("", async (UnitsOfMeasuresService service) => await service.GetUnitsOfMeasuresAsync());
            group.MapGet("{id}", async (UnitsOfMeasuresService service, Guid id) => await service.GetUnitOfMeasureAsync(id));
            group.MapPost("", async (UnitsOfMeasuresService service, CreateUnitOfMeasureDto newUnitOfMeasure) => await service.CreateUnitOfMeasureAsync(newUnitOfMeasure));
            group.MapPut("{id}", async (UnitsOfMeasuresService service, Guid id, UnitOfMeasure unitOfMeasure) => await service.UpdateUnitOfMeasureAsync(unitOfMeasure));
            group.MapDelete("{id}", async (UnitsOfMeasuresService service, Guid id) => await service.DeleteUnitOfMeasureAsync(id));
        }
    }
}
