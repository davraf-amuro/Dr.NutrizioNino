using Dr.NutrizioNino.Api.Interfaces;
using Dr.NutrizioNino.Api.Models;
using Microsoft.Extensions.Options;

public class ValidatorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidatorMiddleware> _logger;
    private readonly IAuthorizationTokens _authorizationTokens;

    private readonly string authorizationKey = "internal-authorization";
    public ValidatorMiddleware(RequestDelegate next, IOptions<AuthorizationTokens> tokens, ILogger<ValidatorMiddleware> logger)
    {
        _next = next;
        _authorizationTokens = tokens.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var referer = context.Request.Headers["Referer"].ToString();
        var origin = context.Request.Headers["Origin"].ToString();
        var caller = !string.IsNullOrWhiteSpace(origin) ? origin : referer;

        _logger.LogDebug(
            "Authorization checks - UseOrigins: {UseOrigins}; UseTokens: {UseTokens}",
            _authorizationTokens.UseOrigins,
            _authorizationTokens.UseTokens);

        //permetti di passare a chi non punta alle api "/api/"
        //ad esempio concedi swagger
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Skipping validator for non API path {Path}", context.Request.Path);
            await _next(context);
            return;
        }

        if (_authorizationTokens.UseOrigins && _authorizationTokens.OriginsAllowed?.Count > 0)
        {
            var normalizedCaller = GetNormalizedOrigin(caller);
            if (string.IsNullOrWhiteSpace(normalizedCaller))
            {
                await WriteUnauthorizedAsync(context, "Invalid origin: you shall not pass!");
                _logger.LogWarning("Origin validation failed: caller origin missing for {Path}", context.Request.Path);
                return;
            }

            var allowedOrigin = _authorizationTokens.OriginsAllowed.Any(allowed =>
                normalizedCaller.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));

            if (!allowedOrigin)
            {
                await WriteUnauthorizedAsync(context, "Invalid origin: you shall not pass!");
                _logger.LogWarning(
                    "Origin validation failed for {Path}. Caller: {CallerOrigin}",
                    context.Request.Path,
                    normalizedCaller);
                return;
            }

            _logger.LogTrace("Origin validation passed for {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogTrace("Origin validation disabled");
        }

        if (_authorizationTokens.UseTokens && _authorizationTokens.TokensAllowed?.Count > 0)
        {
            if (!context.Request.Headers.ContainsKey(authorizationKey))
            {
                await WriteUnauthorizedAsync(context, "Authorization token is missing: you shall not pass!");
                _logger.LogWarning("Token validation failed: header {HeaderName} missing", authorizationKey);
                return;
            }

            var token = context.Request.Headers[authorizationKey].ToString();
            if (string.IsNullOrWhiteSpace(token))
            {
                await WriteUnauthorizedAsync(context, "Authorization token is empty: you shall not pass!");
                _logger.LogWarning("Token validation failed: header {HeaderName} empty", authorizationKey);
                return;
            }

            if (!_authorizationTokens.TokensAllowed.Contains(token))
            {
                await WriteUnauthorizedAsync(context, "Invalid authorization token: you shall not pass!");
                _logger.LogWarning("Token validation failed for path {Path}", context.Request.Path);
                return;
            }

            _logger.LogTrace("Token validation passed for {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogTrace("Token validation disabled");
        }

        await _next(context);
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync(message);
    }

    private static string GetNormalizedOrigin(string caller)
    {
        if (string.IsNullOrWhiteSpace(caller))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(caller, UriKind.Absolute, out var uri))
        {
            return uri.Authority;
        }

        return caller
            .Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

}
