namespace Dr.NutrizioNino.Web.Services
{
    public class ConfigurationService(IConfiguration configuration)
    {
        private string? _baseUri;
        public string? GetBaseUri()
        {
            if (_baseUri == null)
            {
                _baseUri = configuration.GetValue<string>("DrNutrizioNinoBaseUri");
            }
            return _baseUri;
        }
    }
}
