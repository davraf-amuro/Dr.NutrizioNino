using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;

namespace Dr.NutrizioNino.Api.Infrastructure.Extensions;

public static class NutrientExtensions
{
    public static Expression<Func<Nutrient, NutrientInfo>> ToNutrientInfo { get; } =
        source => new NutrientInfo(source.Id, source.Name, source.PositionOrder, source.DefaultUnitOfMeasureId, source.DefaultQuantity);
}
