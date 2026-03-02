using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;

namespace Dr.NutrizioNino.Api.Middleware;

public class HttpContextLogger(RequestDelegate next, ILogger<HttpContextLogger> logger)
{
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "authorization",
        "cookie",
        "set-cookie",
        "internal-authorization"
    };

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        var referer = context.Request.Headers["Referer"].ToString();
        var origin = context.Request.Headers["Origin"].ToString();
        var protocol = context.Features.Get<ITlsHandshakeFeature>()?.Protocol;
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var safeHeaderNames = context.Request.Headers
            .Select(h => h.Key)
            .Where(h => !SensitiveHeaders.Contains(h))
            .ToArray();

        logger.LogInformation(
            "Incoming request {Method} {Path}{QueryString} from {ClientIp}. ContentType: {ContentType}; ContentLength: {ContentLength}; TLS: {TlsProtocol}; Origin: {Origin}; Referer: {Referer}; UserAgent: {UserAgent}; SafeHeaders: {SafeHeaders}",
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            clientIp,
            context.Request.ContentType,
            context.Request.ContentLength,
            protocol,
            origin,
            referer,
            userAgent,
            string.Join(",", safeHeaderNames));

        await next(context);
    }
}
