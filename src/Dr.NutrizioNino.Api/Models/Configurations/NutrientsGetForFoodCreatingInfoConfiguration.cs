using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations
{
    public partial class NutrientsGetForFoodCreatingInfoConfiguration : IEntityTypeConfiguration<NutrientsGetForFoodCreatingInfo>
    {
        public void Configure(EntityTypeBuilder<NutrientsGetForFoodCreatingInfo> entity)
        {
            entity.ToView("Nutrients_GetForFoodCreating")
                .HasKey(x => x.FoodId);


            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<NutrientsGetForFoodCreatingInfo> entity);
    }
}
