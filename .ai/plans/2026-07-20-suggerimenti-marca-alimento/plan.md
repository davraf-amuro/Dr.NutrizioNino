# Piano: Mostra marca nei suggerimenti "alimento simile"

Data: 2026-07-20
Stato: COMPLETATO

Nota: durante l'esecuzione, `dotnet run` ha rivelato un bug di sintassi C# preesistente e non correlato
in `DrRepository.NutrientExtractionCache.cs:31` (`existing ?? throw;`, WIP non committato dell'utente,
non nel mio scope) — corretto su conferma esplicita dell'utente (`if (existing is not null) { return
existing; } throw;`, preserva lo stack trace originale) per poter completare la verifica end-to-end.

## Obiettivo

In `FoodDetail.vue`, digitando il nome di un nuovo alimento appare una lista di suggerimenti
(alimenti esistenti con nome simile, `/foods/similar`) con pulsante "Usa questo esistente".
Mostra solo il nome — ambiguo con omonimi di marche diverse. Aggiungere la marca accanto al nome.

## Scope — file da modificare, in ordine

1. `Dr.NutrizioNino.Models\Dto\FoodSuggestionDto.cs` — aggiunge `string? BrandDescription`.
2. `src\Dr.NutrizioNino.Api\Infrastructure\DrRepository.Food.cs` — `GetSimilarFoodNamesAsync`,
   projection aggiunge `f.Brand != null ? f.Brand.Name : null` (navigation già presente su `Food`).
3. `src\Dr.NutrizioNino.WebVue\src\Interfaces\foods\FoodSuggestionDto.ts` — aggiunge
   `brandDescription: string | null`.
4. `src\Dr.NutrizioNino.WebVue\src\components\Foods\FoodDetail.vue` — mostra marca accanto al nome
   nel suggerimento (righe 12-16).

## Fuori scope

- Debounce/abort logica ricerca esistente (FoodDetail.vue:344-359)
- `FoodEndpoints.cs` (nessun mapping intermedio da aggiornare per questo endpoint)
- `FoodsList.vue` / dashboard (task precedente, già completato)

## Criteri di verifica

- [x] Backend compila (`dotnet build`) — 0 errori
- [x] Frontend: `vue-tsc --noEmit` pulito — 0 errori
- [x] Verifica end-to-end via curl `/api/v1/foods/similar?query=bevanda` — risposta con `brandDescription`
      valorizzato per entrambi i risultati ("Via Verde Bio", "Prima Tigros")
- [x] Marca mostrata solo se presente (`v-if="s.brandDescription"`), nessun " — " orfano quando null
