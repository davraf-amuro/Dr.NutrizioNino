# Piano — Gestione "Etichetta non riconosciuta"

Stato: COMPLETATO
Data: 2026-06-28
Modello: claude-opus-4-8

## Obiettivo

Quando l'immagine incollata non è una tabella nutrizionale, il provider Vision
restituisce JSON malformato (oggetto singolo invece di array). Oggi questo:
1. lancia `JsonException` non catturata → 500 / crash;
2. salva il JSON rotto in cache **prima** del parse → cache avvelenata (ogni retry ricrasha).

Il sistema deve intercettare il caso e mostrare all'utente il messaggio
**"Etichetta non riconosciuta"** invece di fallire.

## Scope

| File | Modifica |
|------|----------|
| `src/Dr.NutrizioNino.Api/Services/VisionExtractionService.cs` | Hardening parse: `Deserialize` in try/catch `JsonException` → lista vuota. Spostare salvataggio cache **dopo** il parse, solo se ≥1 nutriente estratto (no cache su estrazione vuota). |
| `src/Dr.NutrizioNino.Api/Endpoints/FoodVisionMapping.cs` | Se risultato vuoto → `422 UnprocessableEntity` ProblemDetails con detail "Etichetta non riconosciuta...". Aggiungere `ProducesProblem(422)`. |
| `src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue` | Nel `catch`: se `ApiError.status === 422` → `message.warning(err.message)` (no prefisso "Estrazione fallita"). |

## Perimetro negativo

NON tocco: provider Vision (`Services/Vision/*`), DTO, `DrRepository`, schema/migration DB, prompt di sistema, `foods.api.ts`, `apiClient.ts`.

## Fasi

- [x] F1 — `VisionExtractionService.cs`: try/catch `JsonException` attorno a `Deserialize` in `MapResultAsync` → ritorna `[]` + `LogWarning`.
- [x] F2 — `VisionExtractionService.cs`: ristrutturare `ExtractNutrientsAsync` — parse prima, salva cache solo se `results.Count > 0`; estrazione vuota ritorna `[]` non cachata + `LogWarning`.
- [x] F3 — `FoodVisionMapping.cs`: risultato vuoto → `Results.Problem(422)` detail "Etichetta non riconosciuta. Verifica che l'immagine sia una tabella nutrizionale." + `ProducesProblem(StatusCodes.Status422UnprocessableEntity)`.
- [x] F4 — `FoodDetail.vue`: catch gestisce `ApiError.status === 422` → `message.warning`.
- [x] F5 — Lint gate: `dotnet format` clean sui file modificati (unica voce report = `Nutrient.UnitOfMeasure.cs` CS8618 pre-esistente, fuori scope); `npm run lint` exit 0.
- [x] F6 — Build API exit 0 (0 errori); frontend lint/build OK.

## Criteri di verifica

- [ ] Immagine non-etichetta → nessun crash, utente vede "Etichetta non riconosciuta".
- [ ] Cache NON contiene più righe con JSON non-array (nessun salvataggio su estrazione vuota).
- [ ] Cache-hit su riga avvelenata esistente → ritorna vuoto senza crash (parse hardened).
- [ ] Etichetta valida → comportamento invariato (nutrienti pre-compilati + riconciliazione).
- [ ] `dotnet format --verify-no-changes` exit 0; `npm run lint` exit 0.
