using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dr.NutrizioNino.Api.Models.Configurations;

public partial class FoodConfiguration
{
    partial void OnConfigurePartial(EntityTypeBuilder<Food> entity)
    {
        // IsDish rimosso: i piatti sono ora nella tabella separata Dishes
    }
}
