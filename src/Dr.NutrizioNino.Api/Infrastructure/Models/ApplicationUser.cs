using Microsoft.AspNetCore.Identity;

namespace Dr.NutrizioNino.Api.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public DateOnly DateOfBirth { get; set; }
    public ICollection<UserProfileEntry> ProfileEntries { get; set; } = [];
}
