using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Models
{
    public static class ModelsExtentions
    {
        public static BrandDto AsDto(this Brand brand) => new BrandDto
        (
            brand.Id
            , brand.Name
        );

        
    }
}
