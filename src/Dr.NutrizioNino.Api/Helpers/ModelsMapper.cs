using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Helpers;

public static class ModelsMapper
{
    //public static FoodInfo AsDto(this Food food) => new FoodInfo(
    //    food.Id
    //    , food.Name
    //    , food.Quantity
    //    , food.Barcode
    //    , food.BrandId
    //    , food.Calorie
    //    , new List<NutrientDto>()
    //    );
    public static BrandDto AsDto(this Brand brand) => new BrandDto(brand.Id, brand.Name);

    public static NutrientInfo AsDto(this Nutrient nutrient) => new NutrientInfo(
                                                                                nutrient.Id
                                                                                , nutrient.Name
                                                                                , nutrient.PositionOrder
        , nutrient.DefaultUnitOfMeasureId
        , nutrient.DefaultQuantity
                                                                                );

    public static UnitOfMeasureDto AsDto(this UnitOfMeasure unitOfMeasure) => new UnitOfMeasureDto(
        unitOfMeasure.Id
        , unitOfMeasure.Name
        , unitOfMeasure.Abbreviation
        );

    public static NutrientDto AsDto(this NutrientInfo nutrientInfo) => new NutrientDto(
        nutrientInfo.Id
        , nutrientInfo.Name
        , nutrientInfo.PositionOrder
        , nutrientInfo.DefaultUnitOfMeasureId
        , nutrientInfo.DefaultQuantity
        );
}
