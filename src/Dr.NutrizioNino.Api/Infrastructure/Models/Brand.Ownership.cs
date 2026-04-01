namespace Dr.NutrizioNino.Api.Models;

public partial class Brand
{
    public Guid? OwnerId { get; set; }
    public virtual ApplicationUser? Owner { get; set; }
}
