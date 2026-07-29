# Piano: Log JSON raw Ollama + ignora Energia in kJ

Data: 2026-07-14
Stato: COMPLETATO

## Scope aggiuntivo (richiesto mid-turn, dopo approvazione piano)

L'utente ha chiesto di mostrare i dati raw di Ollama collassati accanto all'immagine in `FoodDetail.vue`.
Per farlo, il contratto dell'endpoint `POST /foods/extract-nutrients` è cambiato da `IList<ExtractedNutrientDto>`
a un nuovo DTO wrapper `ExtractionResultDto(Nutrients, RawJson)` — unico consumer è questa stessa app,
cambio contratto sicuro. File aggiuntivi toccati rispetto allo scope originale:
- `Dr.NutrizioNino.Models/Dto/ExtractionResultDto.cs` (nuovo)
- `src/Dr.NutrizioNino.Api/Endpoints/FoodVisionMapping.cs` (Produces + check `result.Nutrients.Count`)
- `src/Dr.NutrizioNino.WebVue/src/Interfaces/foods/ExtractedNutrientDto.ts` (nuova interfaccia `ExtractionResultDto`)
- `src/Dr.NutrizioNino.WebVue/src/modules/foods/api/foods.api.ts` (tipo di ritorno aggiornato)
- `src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue` (pannello `n-collapse` con JSON raw accanto all'anteprima immagine)

## Obiettivo

1. Loggare il JSON grezzo restituito da Ollama (oggi solo l'eccezione viene loggata su parse fallito, il contenuto raw non è mai visibile — impossibile diagnosticare risposte malformate come quella osservata oggi alle 08:38).
2. Quando arrivano i dati estratti, ignorare sempre le righe Energia in kJ: considerare solo kcal. Decisione utente: se un'etichetta riporta SOLO kJ (nessun kcal stampato), l'energia non viene estratta (nessun fallback di conversione kJ→kcal).

## Contesto rilevante (letto prima di pianificare)

- Piano precedente `2026-06-28-energia-kj-kcal-conversion` (COMPLETATO, superato): aveva introdotto il fallback di conversione DB kJ→kcal per etichette kJ-only. Questo piano lo **disattiva esplicitamente** su richiesta utente odierna.
- Piano precedente `2026-06-28-vision-etichetta-non-riconosciuta`: ha introdotto il try/catch su `JsonException` in `MapResultAsync` — resta invariato, aggiungiamo solo logging.
- Dedup esistente in `MapResultAsync` (righe 205-231) preferisce la riga Matched senza conversione per Energia — con lo scarto diretto delle righe kJ, il dedup su Energia diventa superfluo (resterà comunque attivo per eventuali altri nutrienti duplicati, nessuna modifica necessaria).

## Scope

### File da modificare
- [ ] `src/Dr.NutrizioNino.Api/Services/VisionExtractionService.cs` — 2 modifiche:
  1. Log `rawJson` (dopo `ExtractJsonArray`, prima di `MapResultAsync`) a livello Information con placeholder strutturato.
  2. In `MapResultAsync`, dentro il `foreach (var r in raw)`: skip immediato (continue) se `string.Equals(r.Unit, "kJ", StringComparison.OrdinalIgnoreCase)`, prima di qualunque matching/conversione.

### Perimetro negativo
- Non tocco: prompt di sistema (`BuildSystemPromptAsync`) — Ollama continua a restituire anche kJ, il filtro avviene lato nostro all'arrivo dei dati, come richiesto.
- Non tocco: `UnitConversions` a DB, altre conversioni (g↔gr, mcg, mg, ecc.) — resta tutto invariato.
- Non tocco: FE (`ExtractionReconciliation.vue`, `FoodDetail.vue`) — l'assenza della riga kJ non richiede nuovo stato UI, è semplicemente una riga in meno nell'array restituito.
- Non tocco: `OllamaVisionProvider.cs`, `VisionProviderFactory.cs`, altri provider Vision.
- Non tocco: dedup esistente (righe 205-231) — resta per altri eventuali duplicati, nessuna modifica.

## Fasi

### Fase 1: Log JSON raw Ollama
- **Stato**: [x]
- **Precondizione**: `rawJson` calcolato da `ExtractJsonArray(rawText)` in `ExtractNutrientsAsync`
- **File**: `src/Dr.NutrizioNino.Api/Services/VisionExtractionService.cs`
- **Operazione**: EDIT
- **Azione**: aggiungere `logger.LogInformation("Ollama raw JSON: hash={Hash} provider={Provider} json={Json}", imageHash, providerKey, rawJson);` subito dopo la riga `rawJson = ExtractJsonArray(rawText);`
- **Tool ammessi**: nessuno (solo edit diretto)
- **Verifica passo**: rilettura file — riga di log presente, placeholder strutturati (no string interpolation)

### Fase 2: Ignora Energia in kJ
- **Stato**: [x]
- **Precondizione**: Fase 1 completata e verificata
- **File**: `src/Dr.NutrizioNino.Api/Services/VisionExtractionService.cs`
- **Operazione**: EDIT
- **Azione**: dentro `foreach (var r in raw)` in `MapResultAsync`, come prima istruzione del corpo, aggiungere check che salta (continue) le righe con `r.Unit` uguale a "kJ" (case-insensitive), con commento che spiega la scelta (ignorato su richiesta esplicita, nessun fallback kJ-only)
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura file — check presente prima del matching, build API 0 errori

### Fase 3: Build e verifica
- **Stato**: [x]
- **Precondizione**: Fasi 1-2 completate
- **File**: nessuno (solo comando)
- **Operazione**: nessuna (verifica)
- **Azione**: `dotnet build src/Dr.NutrizioNino.Api/Dr.NutrizioNino.Api.csproj` + `vue-tsc --noEmit` (per lo scope aggiuntivo FE)
- **Tool ammessi**: Bash
- **Verifica passo**: 0 errori di compilazione (warning preesistenti ignorati)

## Criteri di verifica finale
- [x] Log contiene il JSON raw di Ollama ad ogni estrazione (riuscita o fallita) — `VisionExtractionService.cs`, LogInformation con placeholder strutturati
- [x] Riga Energia con unit="kJ" non appare mai nel risultato finale, indipendentemente dalla presenza di kcal — `continue` incondizionato in `MapResultAsync`
- [x] Etichetta con Energia solo in kcal → comportamento invariato (Matched, verde) — logica dedup/match non toccata per kcal
- [x] Etichetta ipotetica con SOLO kJ → nessuna riga Energia estratta (nessun crash, nessuna riga fantasma) — filtro scarta la riga prima del matching, nessuna eccezione
- [x] Build API 0 errori — `dotnet build` pulito
- [x] Scope aggiuntivo: pannello JSON raw collassabile in FE — `vue-tsc --noEmit` 0 errori, unico consumer dell'endpoint aggiornato coerentemente (BE+FE)

⚠️ Non verificato end-to-end con l'app in esecuzione (dll bloccata da debugger attaccato in sessione precedente) — verifica manuale consigliata: riavviare debug, ripassare l'etichetta reale da `/foods`, controllare log per riga "Ollama raw JSON" e assenza di kJ nell'output/UI.
