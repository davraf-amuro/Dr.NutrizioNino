using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dr.NutrizioNino.Models.Dto
{
    public record FoodCreationTemplateDto(
        Guid Id
        , string Name
        , decimal Quantity  
        , string? Barcode
        , Guid? BrandId
        , int Calorie
        , IList<NutrientDto> Nutrients
        ) : FoodDto(Id
            , Name
            , Quantity
            , Barcode
            , BrandId
            , Calorie)
    {
    }
}
