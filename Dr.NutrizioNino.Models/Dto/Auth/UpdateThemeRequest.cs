using System.ComponentModel.DataAnnotations;

namespace Dr.NutrizioNino.Models.Dto.Auth;

public record UpdateThemeRequest([Required] string Theme);
