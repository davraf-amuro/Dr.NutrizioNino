using Dr.NutrizioNino.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Dr.NutrizioNino.Api.Models;

public partial class DrNutrizioNinoContext
{
    public virtual DbSet<SciaudoneCard>? SciaudoneCards { get; set; }
}
