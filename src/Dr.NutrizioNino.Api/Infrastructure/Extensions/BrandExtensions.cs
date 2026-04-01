using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Infrastructure.Extensions;

public static class BrandExtensions
{
    public static Expression<Func<Brand, BrandDto>> ToBrandDto { get; } =
        source => new BrandDto(source.Id, source.Name);
}
