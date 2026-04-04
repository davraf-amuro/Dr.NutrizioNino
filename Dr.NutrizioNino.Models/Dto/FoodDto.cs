using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dr.NutrizioNino.Models.Dto;

public record FoodDto(
    Guid Id
    , string Name
    , decimal Quantity
    , string? Barcode
    , Guid? BrandId
    , IList<NutrientDto> Nutrients
    )
{ }
