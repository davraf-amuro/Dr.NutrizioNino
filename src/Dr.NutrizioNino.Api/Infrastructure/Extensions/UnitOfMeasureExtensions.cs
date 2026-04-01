using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Infrastructure.Extensions;

public static class UnitOfMeasureExtensions
{
    public static Expression<Func<UnitOfMeasure, UnitOfMeasureDto>> ToUnitOfMeasureDto { get; } =
        source => new UnitOfMeasureDto(source.Id, source.Name, source.Abbreviation);
}
