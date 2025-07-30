namespace Dr.NutrizioNino.Api.Middleware;
//public class SecurityTokenFilter : IEndpointFilter
//{
//    private readonly ITokenService _tokenService;

//    public SecurityTokenFilter(ITokenService tokenService)
//    {
//        _tokenService = tokenService;
//    }

//    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
//    {
//        var httpContext = context.HttpContext;
//        var requireToken = httpContext.GetEndpoint()?.Metadata.GetMetadata<RequireTokenAttribute>();

//        if (requireToken != null)
//        {
//            var token = await _tokenService.GetTokenAsync();
//            httpContext.Request.Headers.Authorization = $"Bearer {token}";
//        }

//        return await next(context);
//    }
//}
