using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations
{
    public partial class FoodDashboardConfiguration : IEntityTypeConfiguration<FoodDashboard>
    {
        public void Configure(EntityTypeBuilder<FoodDashboard> entity)
        {
            entity.ToView("Foods_Dashboard");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.HasKey(e => e.Id);

            OnConfigurePartial(entity);
        }

        partial void OnConfigurePartial(EntityTypeBuilder<FoodDashboard> entity);
    }
}