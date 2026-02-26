namespace Dr.NutrizioNino.Api.Infrastructure.Models;

/// <summary>
/// Record in sola lettura usato come dto sia per la creazione che per la modifica di un alimento.
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Quantity"></param>
/// <param name="Barcode"></param>
/// <param name="BrandId"></param>
/// <param name="Calorie"></param>
/// <param name="Nutrients"></param>
public record FoodInfo(
    Guid Id
    , string Name
    , decimal Quantity
    , string? Barcode
    , Guid? BrandId
    , int Calorie
    , Guid UnitOfMeasureId
    , IList<NutrientsGetForFoodCreatingInfo> Nutrients
    )
{ }
