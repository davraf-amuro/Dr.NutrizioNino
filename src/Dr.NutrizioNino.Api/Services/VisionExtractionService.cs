using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dr.NutrizioNino.Api.Infrastructure;
using Dr.NutrizioNino.Api.Infrastructure.Models;
using Dr.NutrizioNino.Api.Services.Vision;
using Dr.NutrizioNino.Models.Dto;

namespace Dr.NutrizioNino.Api.Services;

public class VisionExtractionService(
    DrRepository drRepository,
    VisionProviderFactory providerFactory,
    ILogger<VisionExtractionService> logger)
{
    /// <summary>Builds the system prompt injecting canonical nutrient names and units from DB.</summary>
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
            - "name": usa ESCLUSIVAMENTE e LETTERALMENTE uno dei nomi canonici dall'elenco sopra — nessuna variazione, abbreviazione, traduzione o riformulazione
            - "unit": usa ESATTAMENTE l'unità canonica indicata tra parentesi nell'elenco (es. "gr", "mg", "kcal", "mcg")
            - "value": il valore numerico nell'unità canonica (0 se non presente o non rilevabile)
            - "confidenceScore": valore tra 0.0 e 1.0
            Rispondi SOLO con il JSON array, senza testo aggiuntivo.
            Formato: [{"name":"Energia","value":250,"unit":"kcal","confidenceScore":0.95},{"name":"Carboidrati","value":30.5,"unit":"gr","confidenceScore":0.98}]
            """;
    }

    /// <summary>Extracts nutrients from a base64 image using the specified LLM provider; uses cache keyed by hash+provider.</summary>
    public async Task<IList<ExtractedNutrientDto>> ExtractNutrientsAsync(
        string base64Image, string mediaType, string providerKey, CancellationToken ct = default)
    {
        var imageHash = ComputeHash(base64Image);

        // Cache lookup per hash + provider
        var cached = await drRepository.GetCacheByHashAsync(imageHash, providerKey, ct).ConfigureAwait(false);
        if (cached is not null)
        {
            logger.LogInformation("Cache hit: hash={Hash} provider={Provider}", imageHash, providerKey);
            return await MapResultAsync(cached.ExtractedJson, ct).ConfigureAwait(false);
        }

        var systemPrompt = await BuildSystemPromptAsync(ct).ConfigureAwait(false);
        var provider = providerFactory.Resolve(providerKey);

        logger.LogInformation("Estrazione nutrienti: hash={Hash} provider={Provider}", imageHash, providerKey);

        string rawJson;
        try
        {
            var rawText = await provider.ExtractRawJsonAsync(base64Image, systemPrompt, ct).ConfigureAwait(false);
            rawJson = ExtractJsonArray(rawText);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Errore nella chiamata al provider {Provider}", providerKey);
            return [];
        }

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            logger.LogWarning("Risposta vuota dal provider {Provider}", providerKey);
            return [];
        }

        // Salva in cache
        await drRepository.SaveExtractionCacheAsync(new NutrientExtractionCache
        {
            Id = Guid.NewGuid(),
            ImageHash = imageHash,
            ProviderKey = providerKey,
            ExtractedJson = rawJson,
            ConfidenceScore = 0f,
            CreatedAt = DateTime.UtcNow
        }, ct).ConfigureAwait(false);

        return await MapResultAsync(rawJson, ct).ConfigureAwait(false);
    }

    private static string ExtractJsonArray(string text)
    {
        var start = text.IndexOf('[');
        var end = text.LastIndexOf(']');
        if (start >= 0 && end > start)
        {
            return text[start..(end + 1)];
        }

        return text;
    }

    private record NutrientLookup(Guid Id, string Name, string UnitaMisura);

    /// <summary>Maps raw JSON to ExtractedNutrientDto list, resolving aliases and computing ExtractionStatus.</summary>
    private async Task<IList<ExtractedNutrientDto>> MapResultAsync(string json, CancellationToken ct)
    {
        var knownNutrients = await drRepository.GetNutrientsAsync(
            n => new NutrientLookup(n.Id, n.Name, n.UnitaMisura), ct).ConfigureAwait(false);

        var aliasMap = await drRepository.GetAllAliasesAsync(ct).ConfigureAwait(false);

        var raw = JsonSerializer.Deserialize<List<RawExtracted>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        var results = new List<ExtractedNutrientDto>();

        foreach (var r in raw)
        {
            // Cerca per nome canonico, poi per alias (O(1), nessun round-trip DB nel loop)
            var matched = knownNutrients.FirstOrDefault(n =>
                string.Equals(n.Name, r.Name, StringComparison.OrdinalIgnoreCase));

            if (matched is null && aliasMap.TryGetValue(r.Name, out var alias))
            {
                matched = knownNutrients.FirstOrDefault(n => n.Id == alias.NutrientId);
            }

            decimal? convertedValue = null;
            string? canonicalUnit = null;
            var status = ExtractionStatus.Unrecognized;

            if (matched is not null)
            {
                status = ExtractionStatus.IncompleteMatch;

                if (!string.IsNullOrEmpty(r.Unit))
                {
                    if (string.IsNullOrEmpty(matched.UnitaMisura) || matched.UnitaMisura == r.Unit)
                    {
                        status = ExtractionStatus.Matched;
                    }
                    else
                    {
                        try
                        {
                            convertedValue = UnitConversionService.Convert(r.Value, r.Unit, matched.UnitaMisura);
                            canonicalUnit = matched.UnitaMisura;
                            status = ExtractionStatus.Matched;
                        }
                        catch (NotSupportedException ex)
                        {
                            logger.LogWarning(ex, "Conversione non supportata: {From} → {To}", r.Unit, matched.UnitaMisura);
                        }
                    }
                }
            }

            results.Add(new ExtractedNutrientDto(
                Name: r.Name,
                Value: r.Value,
                Unit: r.Unit,
                ConvertedValue: convertedValue,
                CanonicalUnit: canonicalUnit,
                MatchedNutrientId: matched?.Id,
                ConfidenceScore: r.ConfidenceScore,
                Status: status));
        }

        return results;
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private record RawExtracted(string Name, decimal Value, string Unit, float ConfidenceScore);
}
