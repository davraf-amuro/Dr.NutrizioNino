using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Infrastructure.Extensions;

public static class CategoryExtensions
{
    public static Expression<Func<Category, CategoryDto>> ToCategoryDto { get; } =
        source => new CategoryDto(source.Id, source.Name);
}
