using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Infrastructure
{
    internal static class ModelsFactory
    {
        internal static async Task<Food> CreateFood()
        {
            var newFood = new Food() { Id = Guid.NewGuid() };
            newFood.FoodsNutrients = new List<FoodNutrient>();
            return newFood;
        }

        internal static async Task<Nutrient> CreateNutrient(CreateNutrientDto newNutrientDto)
        {
            return new Nutrient
            {
                Id = Guid.NewGuid(),
                Name = newNutrientDto.Name,
            };
        }

        public static async Task<Brand> CreateBrand(CreateBrandDto newBrandDto)
        {
            return new Brand
            {
                // assegna un nuovo guid a id
                Id = Guid.NewGuid(),
                Name = newBrandDto.Name,
            };
        }

        public static async Task<UnitOfMeasure> CreateUnitOfMeasure(CreateUnitOfMeasureDto newUnitOfMeasure)
        {
            return new UnitOfMeasure
            {
                Id = Guid.NewGuid(),
                Name = newUnitOfMeasure.Name,
                Abbreviation = newUnitOfMeasure.Abbreviation
            };
        }
    }
}
