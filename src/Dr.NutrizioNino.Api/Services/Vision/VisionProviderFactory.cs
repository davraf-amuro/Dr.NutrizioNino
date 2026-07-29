namespace Dr.NutrizioNino.Api.Services.Vision;

public class VisionProviderFactory(IEnumerable<IVisionProvider> providers, ILogger<VisionProviderFactory> logger)
{
    private readonly Dictionary<string, IVisionProvider> _providers =
        providers.ToDictionary(p => p.ProviderKey, StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns the provider for the given key; falls back to 'ollama' if key is unknown.</summary>
    public IVisionProvider Resolve(string providerKey)
    {
        if (_providers.TryGetValue(providerKey, out var provider))
        {
            return provider;
        }

        logger.LogWarning("Provider '{ProviderKey}' non trovato, fallback su ollama", providerKey);
        return _providers["ollama"];
    }

    /// <summary>Returns metadata for all registered providers.</summary>
    public IReadOnlyList<VisionProviderInfo> GetAll() =>
        _providers.Values
            .Select(p => new VisionProviderInfo(p.ProviderKey, ProviderLabel(p.ProviderKey)))
            .ToList();

    private static string ProviderLabel(string key) => key switch
    {
        "ollama" => "Ollama (locale)",
        "claude" => "Claude API",
        "azure" => "Azure AI Foundry",
        _ => key
    };
}

public record VisionProviderInfo(string Key, string Label);
