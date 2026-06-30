using System.Text.Json;

namespace Dr.NutrizioNino.Api.Services.Vision;

public class AzureVisionProvider(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AzureVisionProvider> logger) : IVisionProvider
{
    public string ProviderKey => "azure";

    /// <summary>Calls Azure OpenAI chat completions endpoint with vision payload.</summary>
    public async Task<string> ExtractRawJsonAsync(string base64Image, string systemPrompt, CancellationToken ct)
    {
        var endpoint = configuration["Vision:Azure:Endpoint"] ?? string.Empty;
        var apiKey = configuration["Vision:Azure:ApiKey"] ?? string.Empty;
        var deployment = configuration["Vision:Azure:DeploymentName"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(deployment))
        {
            logger.LogError("Vision:Azure config incompleta (Endpoint/ApiKey/DeploymentName)");
            return string.Empty;
        }

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("api-key", apiKey);

        var body = new
        {
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } },
                        new { type = "text", text = "Analizza l'etichetta e restituisci il JSON." }
                    }
                }
            },
            max_tokens = 1024
        };

        var url = $"{endpoint}/openai/deployments/{deployment}/chat/completions?api-version=2024-02-01";

        logger.LogInformation("Azure request: deployment={Deployment} endpoint={Endpoint}", deployment, endpoint);

        var response = await client.PostAsJsonAsync(url, body, ct).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        // Risposta Azure OpenAI: { "choices": [{ "message": { "content": "..." } }] }
        using var doc = JsonDocument.Parse(content);
        if (doc.RootElement.TryGetProperty("choices", out var choices)
            && choices.GetArrayLength() > 0
            && choices[0].TryGetProperty("message", out var message)
            && message.TryGetProperty("content", out var msgContent))
        {
            return msgContent.GetString() ?? string.Empty;
        }

        return content;
    }
}
