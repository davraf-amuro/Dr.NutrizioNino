using Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;
using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Models;

public partial class DrNutrizioNinoContext
{
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<FoodCategory> FoodCategories { get; set; }
}
