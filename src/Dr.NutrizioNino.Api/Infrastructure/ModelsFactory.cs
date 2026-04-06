using Dr.NutrizioNino.Api.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Infrastructure;

internal static class ModelsFactory
{
    internal static Food CreateFood(CreateFoodDto foodDto)
    {
        var newFood = new Food() { Id = Guid.NewGuid() };

        if (foodDto != null)
        {
            newFood.Name = foodDto.Name;
            newFood.BrandId = foodDto.BrandId;
            newFood.Barcode = foodDto.Barcode;
        }

        return newFood;
    }

    internal static Nutrient CreateNutrient(CreateNutrientDto newNutrientDto)
    {
        return new Nutrient
        {
            Id = Guid.NewGuid(),
            Name = newNutrientDto.Name,
            PositionOrder = newNutrientDto.PositionOrder,
            DefaultQuantity = newNutrientDto.DefaultQuantity,
            DefaultUnitOfMeasureId = newNutrientDto.DefaultUnitOfMeasureId ?? Guid.Empty,
        };
    }

    public static Brand CreateBrand(CreateBrandDto newBrandDto)
    {
        return new Brand
        {
            Id = Guid.NewGuid(),
            Name = newBrandDto.Name,
        };
    }

    public static UnitOfMeasure CreateUnitOfMeasure(CreateUnitOfMeasureDto newUnitOfMeasure)
    {
        return new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Name = newUnitOfMeasure.Name,
            Abbreviation = newUnitOfMeasure.Abbreviation
        };
    }
}
