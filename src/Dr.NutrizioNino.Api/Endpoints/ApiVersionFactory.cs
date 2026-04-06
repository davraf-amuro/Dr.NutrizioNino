using Asp.Versioning;

namespace Dr.NutrizioNino.Api.Endpoints;

public class ApiVersionFactory
{
    public static ApiVersion Version1 => new ApiVersion(1);
}
