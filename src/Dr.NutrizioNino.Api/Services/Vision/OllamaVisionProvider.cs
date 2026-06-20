using System.Text.Json;

namespace Dr.NutrizioNino.Api.Services.Vision;

public class OllamaVisionProvider(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<OllamaVisionProvider> logger) : IVisionProvider
{
    public string ProviderKey => "ollama";

    // Ollama è single-threaded per inferenza: serializza le richieste
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>Calls Ollama /api/generate with vision payload; serializes concurrent requests via semaphore.</summary>
    public async Task<string> ExtractRawJsonAsync(string base64Image, string systemPrompt, CancellationToken ct)
    {
        var endpoint = configuration["Vision:Ollama:Endpoint"] ?? "http://localhost:11434";
        var model = configuration["Vision:Ollama:Model"] ?? "qwen2.5vl";

        await _semaphore.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var client = httpClientFactory.CreateClient("ollama");
            var body = new
            {
                model,
                prompt = systemPrompt,
                images = new[] { base64Image },
                stream = false
            };

            logger.LogInformation("Ollama request: model={Model} endpoint={Endpoint}", model, endpoint);

            var response = await client.PostAsJsonAsync($"{endpoint}/api/generate", body, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            // Risposta Ollama: { "response": "testo generato" }
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("response", out var resp))
                return resp.GetString() ?? string.Empty;

            return content;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
