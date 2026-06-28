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
    UnitConversionService unitConversionService,
    ILogger<VisionExtractionService> logger)
{
    /// <summary>Builds the system prompt injecting canonical nutrient names and units from DB.</summary>
    private async Task<string> BuildSystemPromptAsync(CancellationToken ct)
    {
        var nutrients = await drRepository.GetNutrientsAsync(
            n => new { n.Name, n.UnitaMisura }, ct).ConfigureAwait(false);

        var nutrientList = string.Join("\n", nutrients.Select(n => $"- {n.Name} ({n.UnitaMisura})"));

        return $$"""
            Esperto OCR per tabelle nutrizionali. Immagine = tabella nutrizionale alimento.

            Compito: trascrivi ogni nutriente abbinando il VALORE all'UNITA scritta accanto, esattamente come in etichetta. Non convertire, non calcolare, non scegliere: copia.

            Leggi SOLO la colonna "per 100 g" (o "per 100 ml"). Ignora "per porzione", "per pezzo", percentuali "%".

            Se un nutriente ha PIU valori con unita diverse (tipico ENERGIA: "2252 kJ" e "539 kcal"), restituisci un oggetto per OGNI coppia valore+unita: per l'energia DUE oggetti, uno con kJ e uno con kcal.

            Includi i sotto-nutrienti "di cui ..." (es. "di cui acidi grassi saturi", "di cui zuccheri") come righe a se.

            Denominazione - elenco nutrienti canonici (nome + unita canonica tra parentesi):
            {{nutrientList}}
            - Nutriente corrisponde a uno elenco: usa il nome canonico. L'unita resta SEMPRE quella STAMPATA accanto al valore.
            - Nessuna corrispondenza: usa nome e unita come in etichetta.

            Ogni oggetto:
            - "name": nome (canonico se riconosciuto, altrimenti etichetta)
            - "value": numero come stampato nella colonna per 100 g/ml
            - "unit": unita STAMPATA accanto a quel numero
            - "confidenceScore": 1.0 nitido e leggibile; 0.5-0.8 sfocato o parzialmente coperto

            Rispondi SOLO con JSON array, senza testo, senza markdown.
            Formato: [{"name":"Energia","value":2252,"unit":"kJ","confidenceScore":1.0},{"name":"Energia","value":539,"unit":"kcal","confidenceScore":1.0},{"name":"Zuccheri","value":56.3,"unit":"gr","confidenceScore":1.0}]
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
                            convertedValue = await unitConversionService.ConvertAsync(r.Value, r.Unit, matched.UnitaMisura).ConfigureAwait(false);
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

        // Dedup: per un nutriente riconosciuto possono arrivare piu coppie valore+unita
        // (tipico Energia: kJ e kcal). Tieni quella con unita gia canonica (nessuna
        // conversione, es. 539 kcal); altrimenti la prima. Le non riconosciute restano.
        var chosen = results
            .Where(r => r.MatchedNutrientId is not null)
            .GroupBy(r => r.MatchedNutrientId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.FirstOrDefault(r => r.Status == ExtractionStatus.Matched && r.CanonicalUnit is null) ?? g.First());

        var deduped = new List<ExtractedNutrientDto>();
        var seen = new HashSet<Guid>();
        foreach (var r in results)
        {
            if (r.MatchedNutrientId is null)
            {
                deduped.Add(r);
                continue;
            }

            if (seen.Add(r.MatchedNutrientId.Value))
            {
                deduped.Add(chosen[r.MatchedNutrientId.Value]);
            }
        }

        return deduped;
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private record RawExtracted(string Name, decimal Value, string Unit, float ConfidenceScore);
}
