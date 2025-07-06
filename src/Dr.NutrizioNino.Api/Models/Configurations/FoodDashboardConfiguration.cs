using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations
{
    public partial class FoodDashboardConfiguration : IEntityTypeConfiguration<FoodDashboardInfo>
    {
        public void Configure(EntityTypeBuilder<FoodDashboardInfo> entity)
        {
            entity.ToView("Foods_Dashboard");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<FoodDashboardInfo> entity);
    }
}