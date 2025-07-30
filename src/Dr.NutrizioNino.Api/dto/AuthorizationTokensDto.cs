namespace Dr.NutrizioNino.Api.dto
{
    public class AuthorizationTokensDto
    {
        public bool UseTokens { get; set; }
        public List<string> TokensAllowed { get; set; }
        public bool UseOrigins { get; set; }
        public List<string> OriginsAllowed { get; set; }
    }
}
