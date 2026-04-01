using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Infrastructure.Extensions;

public static class SupermarketExtensions
{
    public static Expression<Func<Supermarket, SupermarketDto>> ToSupermarketDto { get; } =
        source => new SupermarketDto(source.Id, source.Name);
}
