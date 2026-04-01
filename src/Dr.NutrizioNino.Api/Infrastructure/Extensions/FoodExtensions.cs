using System.Linq.Expressions;
using Dr.NutrizioNino.Api.Models;

namespace Dr.NutrizioNino.Api.Infrastructure.Extensions;

/// <summary>
/// Proiezioni Expression per l'entità Food.
/// Food è un aggregato complesso: le query di dashboard usano la vista SQL Foods_Dashboard.
/// Questo file fornisce selectors per query semplici (esistenza, naming).
/// </summary>
public static class FoodExtensions
{
    public static Expression<Func<Food, Guid>> ToId { get; } =
        source => source.Id;

    public static Expression<Func<Food, string>> ToName { get; } =
        source => source.Name;
}
