using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class VisionExtractionService(
    DrRepository drRepository,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<VisionExtractionService> logger)
{
    private async Task<string> BuildSystemPromptAsync(CancellationToken ct)
    {
        var nutrients = await drRepository.GetNutrientsAsync(
            n => new { n.Name, n.UnitaMisura }, ct).ConfigureAwait(false);

        var nutrientList = string.Join("\n", nutrients.Select(n => $"- {n.Name} ({n.UnitaMisura})"));

        return $$"""
            Sei un esperto di nutrizione. Analizza la seguente lista di nutrienti e restituisci un JSON array con i valori tipici per un alimento generico da 100g.
            Usa ESCLUSIVAMENTE i seguenti nutrienti. Per ciascuno è indicato il nome canonico e l'unità di misura canonica (tra parentesi):
            {{nutrientList}}
            Per ogni nutriente:
            - "name": usa ESATTAMENTE il nome canonico dall'elenco (rispetta maiuscole e spazi)
            - "unit": usa ESATTAMENTE l'unità canonica indicata tra parentesi nell'elenco (es. "gr", "mg", "kcal", "mcg")
            - "value": il valore numerico nell'unità canonica
            - "confidenceScore": valore tra 0.0 e 1.0
            Rispondi SOLO con il JSON array, senza testo aggiuntivo.
            Formato: [{"name":"Energia","value":250,"unit":"kcal","confidenceScore":0.95},{"name":"Carboidrati","value":30.5,"unit":"gr","confidenceScore":0.98}]
            """;
    }

    public async Task<IList<ExtractedNutrientDto>> ExtractNutrientsAsync(string base64Image, string mediaType, CancellationToken ct = default)
    {
        var imageHash = ComputeHash(base64Image);

        // Controlla la cache
        var cached = await drRepository.GetCacheByHashAsync(imageHash, ct).ConfigureAwait(false);
        if (cached is not null)
        {
            logger.LogInformation("Cache hit per hash {Hash}", imageHash);
            return await MapCachedResultAsync(cached.ExtractedJson, ct).ConfigureAwait(false);
        }

        var endpoint = configuration["LocalAi:Endpoint"] ?? "http://localhost:8000/generate";
        logger.LogInformation("Estrazione nutrienti da immagine (hash={Hash}) via {Endpoint}", imageHash, endpoint);

        var systemPrompt = await BuildSystemPromptAsync(ct).ConfigureAwait(false);

        var httpClient = httpClientFactory.CreateClient();
        var requestBody = new { prompt = systemPrompt, image = base64Image, max_new_tokens = 1024 };

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(endpoint, requestBody, ct).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Errore nella chiamata all'AI locale {Endpoint}", endpoint);
            return [];
        }

        var rawJson = await ParseGeneratedTextAsync(response, ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            logger.LogWarning("Risposta vuota dall'AI locale");
            return [];
        }

        // Salva in cache
        await drRepository.SaveExtractionCacheAsync(new NutrientExtractionCache
        {
            Id = Guid.NewGuid(),
            ImageHash = imageHash,
            ExtractedJson = rawJson,
            ConfidenceScore = 0f,
            CreatedAt = DateTime.UtcNow
        }, ct).ConfigureAwait(false);

        return await MapCachedResultAsync(rawJson, ct).ConfigureAwait(false);
    }

    private static async Task<string> ParseGeneratedTextAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var content = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        // Forma: [{"generated_text": "..."}]
        if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
        {
            var first = root[0];
            if (first.TryGetProperty("generated_text", out var gt))
                return ExtractJsonArray(gt.GetString() ?? string.Empty);
        }

        // Forma: {"generated_text": "..."}
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("generated_text", out var gtObj))
            return ExtractJsonArray(gtObj.GetString() ?? string.Empty);

        // Risposta è già JSON diretto
        return content;
    }

    private static string ExtractJsonArray(string text)
    {
        var start = text.IndexOf('[');
        var end = text.LastIndexOf(']');
        if (start >= 0 && end > start)
            return text[start..(end + 1)];
        return text;
    }

    private record NutrientLookup(Guid Id, string Name, string UnitaMisura);

    private async Task<IList<ExtractedNutrientDto>> MapCachedResultAsync(string json, CancellationToken ct)
    {
        var knownNutrients = await drRepository.GetNutrientsAsync(
            n => new NutrientLookup(n.Id, n.Name, n.UnitaMisura), ct).ConfigureAwait(false);

        var raw = JsonSerializer.Deserialize<List<RawExtracted>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        return raw.Select(r =>
        {
            var matched = knownNutrients.FirstOrDefault(n =>
                string.Equals(n.Name, r.Name, StringComparison.OrdinalIgnoreCase));

            decimal? convertedValue = null;
            string? canonicalUnit = null;

            if (matched is not null && !string.IsNullOrEmpty(matched.UnitaMisura) && matched.UnitaMisura != r.Unit)
            {
                try
                {
                    convertedValue = UnitConversionService.Convert(r.Value, r.Unit, matched.UnitaMisura);
                    canonicalUnit = matched.UnitaMisura;
                }
                catch (NotSupportedException ex)
                {
                    logger.LogWarning(ex, "Conversione non supportata: {From} → {To}", r.Unit, matched.UnitaMisura);
                }
            }

            return new ExtractedNutrientDto(
                Name: r.Name,
                Value: r.Value,
                Unit: r.Unit,
                ConvertedValue: convertedValue,
                CanonicalUnit: canonicalUnit,
                MatchedNutrientId: matched?.Id,
                ConfidenceScore: r.ConfidenceScore);
        }).ToList();
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private record RawExtracted(string Name, decimal Value, string Unit, float ConfidenceScore);
}
