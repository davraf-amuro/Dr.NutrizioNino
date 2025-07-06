using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dr.NutrizioNino.Models.Dto
{
    public record NutrientDto(
        Guid Id
        , string Name
        , int PositionOrder
        , Guid DefaultUnitOfMeasureId
        , decimal DefaultQuantity
        );

}
