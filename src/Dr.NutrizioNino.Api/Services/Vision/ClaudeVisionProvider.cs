using System.Net.Http.Headers;
using System.Text.Json;

namespace Dr.NutrizioNino.Api.Services.Vision;

public class ClaudeVisionProvider(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<ClaudeVisionProvider> logger) : IVisionProvider
{
    public string ProviderKey => "claude";

    /// <summary>Calls Anthropic Messages API with vision (base64 image block).</summary>
    public async Task<string> ExtractRawJsonAsync(string base64Image, string systemPrompt, CancellationToken ct)
    {
        var apiKey = configuration["Vision:Claude:ApiKey"] ?? string.Empty;
        var model = configuration["Vision:Claude:Model"] ?? "claude-haiku-4-5-20251001";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogError("Vision:Claude:ApiKey non configurata");
            return string.Empty;
        }

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new
        {
            model,
            max_tokens = 1024,
            system = systemPrompt,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "image", source = new { type = "base64", media_type = "image/jpeg", data = base64Image } },
                        new { type = "text", text = "Analizza l'etichetta e restituisci il JSON." }
                    }
                }
            }
        };

        logger.LogInformation("Claude request: model={Model}", model);

        var response = await client.PostAsJsonAsync("https://api.anthropic.com/v1/messages", body, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        // Risposta Claude: { "content": [{ "type": "text", "text": "..." }] }
        using var doc = JsonDocument.Parse(content);
        if (doc.RootElement.TryGetProperty("content", out var contentArr)
            && contentArr.GetArrayLength() > 0
            && contentArr[0].TryGetProperty("text", out var text))
        {
            return text.GetString() ?? string.Empty;
        }

        return content;
    }
}
