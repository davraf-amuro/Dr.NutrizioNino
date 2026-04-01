namespace Dr.NutrizioNino.Api.Models;

public partial class Food
{
    public Guid? OwnerId { get; set; }
    public virtual ApplicationUser? Owner { get; set; }
}
