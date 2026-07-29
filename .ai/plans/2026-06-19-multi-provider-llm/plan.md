# Piano: Multi-provider LLM + Riconciliazione nutrienti

**Data:** 2026-06-19
**Stato:** COMPLETATO
**Branch:** dev

---

## Contesto

Sostituire il server Python custom (`server.py` FastAPI + Qwen2-VL-2B, avvio manuale, CPU-only)
con Ollama (`qwen2.5vl` già installato) e aggiungere supporto multi-provider LLM configurabile.
Parallelamente: migliorare il feedback post-estrazione (nutriente parziale vs non riconosciuto),
aggiungere riconciliazione alias, e correggere il bug che non aggiornava l'unità di misura.

**Scelte utente:**
- Alias nutrienti: globali (UNIQUE su AiName)
- Selezione provider: UI per-richiesta
- Provider da implementare: Ollama + Claude API + Azure AI Foundry

---

## Fase 1 — Database migrations [x]

### 1a. `NutrientAlias`
File: `schema-migrations/2026-06-19_nutrient-alias.sql`
Eseguita con sqlcmd.

### 1b. `ProviderKey` su `NutrientExtractionCache`
File: `schema-migrations/2026-06-19_cache-provider-key.sql`
Eseguita con sqlcmd. Drop UQ_NutrientExtractionCache_ImageHash, UNIQUE(ImageHash, ProviderKey).

---

## Fase 2 — Backend: modelli e DTO [x]

- `Dr.NutrizioNino.Models/Dto/ExtractionStatus.cs` — NEW enum `Matched | IncompleteMatch | Unrecognized`
- `Dr.NutrizioNino.Models/Dto/ExtractedNutrientDto.cs` — aggiunto campo `ExtractionStatus Status`
- `src/.../Models/NutrientExtractionCache.cs` — aggiunto `ProviderKey`
- `src/.../Models/NutrientAlias.cs` — NEW entity
- `src/.../DrNutrizioNinoContext.cs` — aggiunto `DbSet<NutrientAlias>`

---

## Fase 3 — Backend: IVisionProvider + implementazioni [x]

- `src/.../Services/Vision/IVisionProvider.cs` — interfaccia con `ProviderKey` + `ExtractRawJsonAsync`
- `src/.../Services/Vision/OllamaVisionProvider.cs` — `POST /api/generate`, `SemaphoreSlim(1,1)`
- `src/.../Services/Vision/ClaudeVisionProvider.cs` — Anthropic Messages API vision
- `src/.../Services/Vision/AzureVisionProvider.cs` — Azure OpenAI chat completions vision
- `src/.../Services/Vision/VisionProviderFactory.cs` — dictionary keyed by `ProviderKey`, fallback ollama

---

## Fase 4 — Backend: VisionExtractionService refactoring [x]

- Primary constructor: `DrRepository + VisionProviderFactory + ILogger`
- Cache lookup con `(ImageHash, ProviderKey)`
- Alias lookup prima di classificare
- `ExtractionStatus` calcolato server-side

### Repository
- `DrRepository.NutrientExtractionCache.cs` — `GetCacheByHashAsync(hash, providerKey, ct)`
- `DrRepository.NutrientAlias.cs` — NEW: `GetAliasByAiNameAsync`, `SaveAliasAsync`

---

## Fase 5 — Backend: endpoint [x]

- `FoodVisionMapping.cs` — request include `ProviderKey`, nessun timeout fisso
- `VisionProvidersMapping.cs` — `GET /api/v1/vision/providers`
- `NutrientAliasMapping.cs` — `POST /api/v1/nutrients/aliases`, rate limit "aliases" 10/min
- `Program.cs` — registrazione provider, factory, rate limiter aliases

---

## Fase 6 — Cleanup [x]

- Eliminati: `server.py`, `test_llm.py`, `test_openvino.py`
- `appsettings.local.json`: sostituito `LocalAi:Endpoint` con sezione `Vision:{ Ollama, Claude, Azure }`

---

## Fase 7 — Frontend: composable e utilità [x]

- `src/WebVue/src/composables/useExtractionTimer.ts` — timer semaforo verde/arancio/rosso
- `src/WebVue/src/Interfaces/foods/ExtractedNutrientDto.ts` — aggiunto `ExtractionStatus` type + campo
- `src/WebVue/src/modules/foods/api/foods.api.ts` — `extractNutrientsFromImage(base64, providerKey, mediaType, signal?)`, `getVisionProviders()`, `saveNutrientAlias()`

---

## Fase 8 — Frontend: componenti [x]

- `LlmProviderSelect.vue` — dropdown provider da `GET /vision/providers`, v-model
- `ExtractionReconciliation.vue` — Caso A (IncompleteMatch ⚠️), Caso B (Unrecognized → alias)
- `FoodDetail.vue` — cronometro + Annulla (AbortController), Rivaluta, Apri in nuova finestra,
  fix UoM (aggiorna anche `unitOfMeasureId` oltre a `quantity`)

---

## Fase 9 — Verifica [x]

- `dotnet build` → 0 errori (87 warning IDE0011 preesistenti, fuori scope)
- `npm run lint` → 0 violazioni (ESLint --fix, exit 0)

### Da verificare manualmente

- [ ] Flusso incolla immagine → Ollama → nutrienti aggiornati (valore + UoM)
- [ ] Cronometro verde → arancio → rosso
- [ ] Pulsante Annulla interrompe fetch
- [ ] Rivaluta con provider diverso
- [ ] Apri in nuova finestra
- [ ] Riconciliazione: alias confermato persistito
- [ ] Log API: no errori `localhost:8000`, presenza "Cache hit" su seconda incolla

---

## Perimetro negativo

Non toccato: auth, rate limiter esistente, `FoodNutrientInput.vue`, altre sezioni app.
