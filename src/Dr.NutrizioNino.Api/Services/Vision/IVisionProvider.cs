namespace Dr.NutrizioNino.Api.Services.Vision;

public interface IVisionProvider
{
    string ProviderKey { get; }

    /// <summary>Sends base64 image + system prompt to the LLM and returns raw JSON text with extracted nutrients.</summary>
    Task<string> ExtractRawJsonAsync(string base64Image, string systemPrompt, CancellationToken ct);
}
