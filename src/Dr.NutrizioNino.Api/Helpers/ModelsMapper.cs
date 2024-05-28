using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Helpers
{
    public static class ModelsMapper
    {
        public static BrandDto AsDto(this Brand brand) => new BrandDto(brand.Id, brand.Name);

        public static FoodDto AsDto(this Food food) => new FoodDto(food.Id, food.Name);

        public static NutrientDto AsDto(this Nutrient nutrient) => new NutrientDto(
                                                                                    nutrient.Id
                                                                                    , nutrient.Name
                                                                                    , nutrient.PositionOrder
                                                                                    );

        public static UnitOfMeasureDto AsDto(this UnitOfMeasure unitOfMeasure) => new UnitOfMeasureDto(
            unitOfMeasure.Id
            , unitOfMeasure.Name
            , unitOfMeasure.Abbreviation
            );
    }
}
