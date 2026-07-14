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
            n => new { n.Name, Unit = n.DefaultUnitOfMeasure.Abbreviation }, ct).ConfigureAwait(false);

        var nutrientList = string.Join("\n", nutrients.Select(n => $"- {n.Name} ({n.Unit})"));

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

    /// <summary>Extracts nutrients from a base64 image using the specified LLM provider; uses cache keyed by hash+provider. Returns both the matched nutrients and the raw JSON for display/diagnostics.</summary>
    public async Task<ExtractionResultDto> ExtractNutrientsAsync(
        string base64Image, string mediaType, string providerKey, CancellationToken ct = default)
    {
        var imageHash = ComputeHash(base64Image);

        // Cache lookup per hash + provider
        var cached = await drRepository.GetCacheByHashAsync(imageHash, providerKey, ct).ConfigureAwait(false);
        if (cached is not null)
        {
            logger.LogInformation("Cache hit: hash={Hash} provider={Provider}", imageHash, providerKey);
            var cachedResults = await MapResultAsync(cached.ExtractedJson, ct).ConfigureAwait(false);
            return new ExtractionResultDto(cachedResults, cached.ExtractedJson);
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
            return new ExtractionResultDto([], string.Empty);
        }

        // Log del JSON grezzo per diagnosticare risposte malformate del provider (non salvato altrove).
        logger.LogInformation("Ollama raw JSON: hash={Hash} provider={Provider} json={Json}", imageHash, providerKey, rawJson);

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            logger.LogWarning("Risposta vuota dal provider {Provider}", providerKey);
            return new ExtractionResultDto([], rawJson);
        }

        // Mappa prima di cachare: un'immagine non-etichetta produce JSON malformato
        // (es. oggetto singolo invece di array) che dà 0 nutrienti. In quel caso NON
        // salviamo in cache, altrimenti ogni retry farebbe cache-hit sullo stesso JSON rotto.
        var results = await MapResultAsync(rawJson, ct).ConfigureAwait(false);

        if (results.Count == 0)
        {
            logger.LogWarning("Etichetta non riconosciuta: nessun nutriente estratto. hash={Hash} provider={Provider}", imageHash, providerKey);
            return new ExtractionResultDto(results, rawJson);
        }

        // Salva in cache solo le estrazioni valide
        await drRepository.SaveExtractionCacheAsync(new NutrientExtractionCache
        {
            Id = Guid.NewGuid(),
            ImageHash = imageHash,
            ProviderKey = providerKey,
            ExtractedJson = rawJson,
            ConfidenceScore = 0f,
            CreatedAt = DateTime.UtcNow
        }, ct).ConfigureAwait(false);

        return new ExtractionResultDto(results, rawJson);
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

    private record NutrientLookup(Guid Id, string Name, string CanonicalUnit);

    /// <summary>Maps raw JSON to ExtractedNutrientDto list, resolving aliases and computing ExtractionStatus.</summary>
    private async Task<IList<ExtractedNutrientDto>> MapResultAsync(string json, CancellationToken ct)
    {
        var knownNutrients = await drRepository.GetNutrientsAsync(
            n => new NutrientLookup(n.Id, n.Name, n.DefaultUnitOfMeasure.Abbreviation), ct).ConfigureAwait(false);

        var aliasMap = await drRepository.GetAllAliasesAsync(ct).ConfigureAwait(false);

        // L'immagine potrebbe non essere un'etichetta: in tal caso il provider rende JSON
        // malformato (oggetto singolo invece di array). Deserialize lancerebbe JsonException:
        // la intercettiamo e trattiamo come "nessun nutriente" (lista vuota), senza propagare.
        List<RawExtracted> raw;
        try
        {
            raw = JsonSerializer.Deserialize<List<RawExtracted>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "JSON estrazione non valido (immagine non riconosciuta come etichetta)");
            return [];
        }

        var results = new List<ExtractedNutrientDto>();

        foreach (var r in raw)
        {
            // Energia in kJ ignorata su richiesta esplicita: si considera solo il valore in kcal, nessun fallback di conversione.
            if (string.Equals(r.Unit, "kJ", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

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

                // Valore 0 = quantità non letta dall'OCR: resta IncompleteMatch anche con unità valida.
                if (!string.IsNullOrEmpty(r.Unit) && r.Value != 0)
                {
                    if (string.IsNullOrEmpty(matched.CanonicalUnit) || matched.CanonicalUnit == r.Unit)
                    {
                        status = ExtractionStatus.Matched;
                    }
                    else
                    {
                        try
                        {
                            convertedValue = await unitConversionService.ConvertAsync(r.Value, r.Unit, matched.CanonicalUnit).ConfigureAwait(false);
                            canonicalUnit = matched.CanonicalUnit;
                            status = ExtractionStatus.Matched;
                        }
                        catch (NotSupportedException ex)
                        {
                            logger.LogWarning(ex, "Conversione non supportata: {From} → {To}", r.Unit, matched.CanonicalUnit);
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
