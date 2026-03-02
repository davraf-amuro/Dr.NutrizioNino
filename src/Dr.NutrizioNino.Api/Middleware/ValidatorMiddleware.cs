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
        var message = string.Empty;

        // Ottieni l'header Referer o Origin
        var referer = context.Request.Headers["Referer"].ToString();
        var origin = context.Request.Headers["Origin"].ToString();
        var caller = !string.IsNullOrEmpty(referer) ? referer : origin;

        _logger.LogInformation($"_authorizationTokens.UseOrigins:{_authorizationTokens.UseOrigins}");
        _logger.LogInformation($"_authorizationTokens.UseTokens:{_authorizationTokens.UseTokens}");

        //permetti di passare a chi non punta alle api "/api/"
        //ad esempio concedi swagger
        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning($"not callinga api! /api/");
            await _next(context);
            return;
        }

        //prima valida l'origine
        if (_authorizationTokens.UseOrigins && _authorizationTokens.OriginsAllowed?.Count > 0)
        {
            var clearOrigin = caller.Replace("http://", "").Replace("https://", "");
            if (!_authorizationTokens.OriginsAllowed.Any(o => clearOrigin.StartsWith(o)))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                message = "Invalid origin: you shall not pass!";
                _logger.LogError(message);
                await context.Response.WriteAsync(message);
                return;
            }
            else
            {
                _logger.LogTrace($"passed with origin {caller}");
            }
        }
        else
        {
            _logger.LogTrace($"AllowedOrigin non abilitato");
        }

        //poi controlla il token
        if (_authorizationTokens.UseTokens && _authorizationTokens.TokensAllowed?.Count > 0)
        {
            if (!context.Request.Headers.ContainsKey(authorizationKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                message = "Authorization token is missing: you shall not pass!";
                _logger.LogError(message);
                await context.Response.WriteAsync(message);
                return;
            }

            var token = context.Request.Headers[authorizationKey].ToString();
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                message = "Authorization token is empty: you shall not pass!";
                _logger.LogError(message);
                await context.Response.WriteAsync(message);
                return;
            }

            if (!_authorizationTokens.TokensAllowed.Contains(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                message = "Invalid authorization token: you shall not pass!";
                _logger.LogError(message);
                await context.Response.WriteAsync(message);
                return;
            }

            _logger.LogTrace($"è passato con token {token}");
        }
        else
        {
            _logger.LogTrace($"AllowedToken non abilitato");
        }

        await _next(context);
    }

}
