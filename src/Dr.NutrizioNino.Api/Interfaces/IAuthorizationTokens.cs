namespace Dr.NutrizioNino.Api.Interfaces;

public interface IAuthorizationTokens
{
    bool UseTokens { get; set; }
    List<string>? TokensAllowed { get; set; }
    bool UseOrigins { get; set; }
    List<string>? OriginsAllowed { get; set; }
}
