
namespace Dr.NutrizioNino.Api.Models;

public partial class Brand
{
    public string Name { get; set; }
    public virtual ICollection<Food> Foods { get; set; } = new List<Food>();
    public Guid Id { get; set; }

}