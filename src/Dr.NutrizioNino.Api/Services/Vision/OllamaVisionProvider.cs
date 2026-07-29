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
                stream = false,
                // format:"json" garantisce solo "JSON valido", qualunque forma: il modello può fermarsi
                // dopo un oggetto singolo e soddisfare comunque il vincolo. Schema esplicito forza l'array.
                format = new
                {
                    type = "array",
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            name = new { type = "string" },
                            value = new { type = "number" },
                            unit = new { type = "string" },
                            confidenceScore = new { type = "number" }
                        },
                        required = new[] { "name", "value", "unit", "confidenceScore" }
                    }
                },
                // num_ctx 2048 era insufficiente: immagine (min 1024 token) + prompt lungo saturavano il contesto,
                // llama.cpp scartava (context-shift) parte delle istruzioni → JSON malformato. Vedi server.log 2026-07-14.
                options = new { num_ctx = 8192, temperature = 0.0 }
            };

            logger.LogInformation("Ollama request: model={Model} endpoint={Endpoint}", model, endpoint);

            var response = await client.PostAsJsonAsync($"{endpoint}/api/generate", body, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

            // Risposta Ollama: { "response": "testo generato" }
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("response", out var resp))
            {
                return resp.GetString() ?? string.Empty;
            }

            return content;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
