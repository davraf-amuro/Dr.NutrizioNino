# Piano — Fix prompt estrazione nutrienti + tuning Ollama

Stato: COMPLETATO
Data: 2026-06-28
Modello: claude-opus-4-8

## Obiettivo

Correggere il system prompt di estrazione nutrienti da immagine e il payload Ollama
per risolvere tre bug osservati su qwen2.5vl:
1. Prompt chiede "valori tipici di alimento generico" invece di leggere l'etichetta in foto.
2. Energia restituita in kJ invece di kcal → **deve considerare solo kcal**.
3. Colonne multiple in etichetta → **deve leggere solo la colonna "per 100 g"**.

Inoltre: hardening output (solo JSON, deterministico) e riduzione VRAM via `num_ctx`.

## Scope

File da modificare:
- `src/Dr.NutrizioNino.Api/Services/VisionExtractionService.cs` — metodo `BuildSystemPromptAsync` (prompt).
- `src/Dr.NutrizioNino.Api/Services/Vision/OllamaVisionProvider.cs` — body richiesta `/api/generate`.

Perimetro negativo (NON toccare):
- Altri provider (Azure, Claude).
- Endpoint, DTO, mapping, contesto DB, migration.
- Logica di cache.

## Fasi

- [ ] Fase 1 — Riscrivere il prompt in `BuildSystemPromptAsync`:
      - Istruzione esplicita: leggi tabella "Informazioni nutrizionali" dall'immagine.
      - Leggi SOLO colonna "per 100 g" (ignora "per porzione" e "%").
      - Energia: riporta SOLO kcal (se presente sia kJ sia kcal, usa kcal; se solo kJ, converti ÷4.184 e arrotonda).
      - Includi sotto-nutrienti "di cui ..." come nutrienti a sé.
      - Scala confidenceScore (1.0 nitido / 0.5-0.8 sfocato / 0.0 assente).
      - Esempio output aggiornato con kcal.
- [ ] Fase 2 — Aggiornare body Ollama in `OllamaVisionProvider.cs`:
      - Aggiungere `format = "json"`, `options = { num_ctx = 2048, temperature = 0.0 }`.
- [ ] Fase 3 — Verifica build: `dotnet build` del progetto Api.
- [ ] Fase 4 — Gate lint pre-push (solo se si pusha): `dotnet format ... --verify-no-changes`.

## Criteri di verifica

- [ ] Prompt nomina esplicitamente "per 100 g" e "kcal".
- [ ] Prompt nomina i sotto-nutrienti "di cui".
- [ ] Body Ollama contiene `format=json` + `num_ctx` + `temperature=0`.
- [ ] `dotnet build` Api senza errori.
