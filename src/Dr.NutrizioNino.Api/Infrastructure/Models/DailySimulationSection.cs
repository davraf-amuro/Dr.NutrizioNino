using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dr.NutrizioNino.Api.Models;

public class DailySimulationSection
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Column(TypeName = "int")]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<DailySimulationEntry> Entries { get; set; } = [];
}
