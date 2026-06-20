# Piano: Vision Extraction — Fix matching + bug silenti

**Data:** 2026-06-20
**Stato:** COMPLETATO — lint backend bloccato da violazioni pre-esistenti (non nei file modificati)
**Branch:** dev

---

## Obiettivo

Correggere i bug che causano "niente a video" dopo l'estrazione Ollama
e migliorare il matching nutrienti/unità per ridurre i falsi Unrecognized/IncompleteMatch.

---

## Scope

**File modificati:**
- `src/Dr.NutrizioNino.Api/Services/VisionExtractionService.cs`
- `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository.Nutrients.cs`
- `src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue`
- `src/Dr.NutrizioNino.WebVue/src/components/Foods/ExtractionReconciliation.vue`

**Perimetro negativo:** auth, endpoint definition, migrations DB, altri service, FoodNutrientInput.vue

---

## Fasi

### Fase 1 — Fix catch silenzioso (FoodDetail.vue) [x]

Problema: catch in `handleImageExtraction` swallow qualsiasi errore non-abort silenziosamente.
Fix: importa `useMessage` da Naive UI, mostra `message.error(...)` per errori non-abort.

Criterio: un errore API deve mostrare notifica visibile all'utente.

---

### Fase 2 — Fix r.Value > 0 (VisionExtractionService.cs) [x]

Problema: condizione `r.Value > 0` impedisce a nutrienti con value=0 di diventare Matched.
Fix: rimuovi il gate `r.Value > 0` — un nutriente riconosciuto con value=0 è dati validi.

Criterio: nutriente "Vitamina B2" value=0, unit="mg" matchato in DB → status Matched.

---

### Fase 3 — Migliora system prompt VERBATIM (VisionExtractionService.cs) [x]

Problema: Ollama produce variazioni di nome ("Vitamina B9 (folati)" vs "Acido Folico") nonostante il system prompt.
Fix: aggiungi istruzione esplicita "usa ESATTAMENTE il nome canonico dall'elenco, nessuna variazione".

Criterio: il system prompt contiene istruzione VERBATIM esplicita.

---

### Fase 4 — Fix N+1 alias lookup (VisionExtractionService.cs + DrRepository) [x]

Problema: `GetAliasByAiNameAsync` chiamato in loop per ogni nutriente = N+1 query.
Fix:
1. Aggiungi `GetAllAliasesAsync()` in `DrRepository.NutrientAlias.cs` → carica tutti gli alias in una query
2. In `MapResultAsync`, carica il dizionario alias una volta sola fuori dal loop

Criterio: `MapResultAsync` fa 2 query DB totali (nutrienti + alias), non N+1.

---

### Fase 5 — Badge confidence in ExtractionReconciliation.vue [x]

Aggiunge badge `n-tag` con:
- `type` dinamico: `success` (≥0.85), `warning` (≥0.60), `error` (<0.60)
- Testo percentuale visibile (es. "92%")
- `aria-label="Confidenza alta: 92%"` (WCAG 1.4.1)

Criterio: ogni nutriente in IncompleteMatch/Unrecognized mostra badge leggibile e accessibile.

---

## Criteri di verifica finale

- [ ] Errore API → messaggio visibile (non silenzioso)
- [ ] Nutriente con value=0 riconosciuto → status Matched
- [ ] System prompt contiene "ESATTAMENTE"
- [ ] `MapResultAsync` carica alias con singola query (nessun loop DB)
- [ ] Badge confidence visibile in ExtractionReconciliation con testo + aria-label
- [ ] `dotnet format --verify-no-changes` → exit 0
- [ ] `npm run lint` → exit 0
