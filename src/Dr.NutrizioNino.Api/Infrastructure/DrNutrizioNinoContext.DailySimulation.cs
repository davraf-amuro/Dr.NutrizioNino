using Dr.NutrizioNino.Api.Infrastructure.Models.Configurations;
using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Models;

public partial class DrNutrizioNinoContext
{
    public virtual DbSet<DailySimulation> DailySimulations { get; set; }
    public virtual DbSet<DailySimulationEntry> DailySimulationEntries { get; set; }
    public virtual DbSet<DailySimulationEntryNutrient> DailySimulationEntryNutrients { get; set; }
}
