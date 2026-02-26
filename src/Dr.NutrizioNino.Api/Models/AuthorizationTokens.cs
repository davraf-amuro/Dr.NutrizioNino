using Dr.NutrizioNino.Api.Interfaces;

namespace Dr.NutrizioNino.Api.Models;

public class AuthorizationTokens : IAuthorizationTokens
{
    public bool UseTokens { get; set; }
    public List<string>? TokensAllowed { get; set; }
    public bool UseOrigins { get; set; }
    public List<string>? OriginsAllowed { get; set; }
}
