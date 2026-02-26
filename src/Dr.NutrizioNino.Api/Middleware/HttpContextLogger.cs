using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Dr.NutrizioNino.Api.Middleware;

public class HttpContextLogger(RequestDelegate next, ILogger<HttpContextLogger> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Bufferizza il corpo della richiesta per permettere letture multiple
        context.Request.EnableBuffering();

        // Leggi il corpo della richiesta
        var bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();

        // Riposiziona il flusso della richiesta all'inizio per i middleware successivi
        context.Request.Body.Position = 0;

        // Ottieni l'indirizzo IP del client
        var clientIp = context.Connection.RemoteIpAddress?.ToString();

        // Costruisci l'URL completo della richiesta
        var fullUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";

        // Ottieni l'header Referer o Origin
        var referer = context.Request.Headers["Referer"].ToString();
        var origin = context.Request.Headers["Origin"].ToString();
        //var caller = !string.IsNullOrEmpty(referer) ? referer : origin;

        // Ottieni il protocollo TLS utilizzato
        var protocol = context.Features.Get<ITlsHandshakeFeature>()?.Protocol;

        // Costruisci il log della richiesta in formato raw
        var requestLog = $"Incoming request {Environment.NewLine}" +
                         $"from IP {clientIp} to {context.Request.Method} {fullUrl} {Environment.NewLine}" +
                         $"Refer: {referer} {Environment.NewLine}" +
                         $"Origin: {origin} {Environment.NewLine}" +
                         $"using TLS Protocol: {protocol} {Environment.NewLine}" +
                         $"Headers: {string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}"))}{Environment.NewLine}" +
                         $"Body: {bodyAsText}{Environment.NewLine}";

        logger.LogInformation(requestLog);

        await next(context);
    }
}
