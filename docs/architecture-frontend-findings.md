# Frontend Architecture Findings

## Executive Summary

Il frontend segue un'architettura feature-oriented (`modules/*/api` + `modules/*/composables`) coerente su tutti i domini (Foods, Brands, Nutrients, Units, Dishes).

Le aree risolte dall'analisi iniziale (2026-03-02): `useAsyncState` condiviso, `ApiError` tipizzato, cache in-memory per dominio, pulizia scaffold dalla shell.

Feature aggiunte (2026-03-25): dominio Piatti completo, ordinamento nutrienti centralizzato via `sortNutrients`.

Rimangono aperti: reattività form Foods (watcher deep), test frontend, convergenza naming.

## Architecture Map

| Layer | File/Cartella |
|-------|---------------|
| App shell + routing | `src/App.vue`, `src/router/index.ts` |
| HTTP core | `src/core/http/apiClient.ts`, `src/core/http/ApiError.ts` |
| Async state + utility | `src/core/composables/useAsyncState.ts`, `src/core/composables/useTableSearch.ts` |
| Utility condivise | `src/core/utils/sortNutrients.ts` |
| Domain — Foods | `src/modules/foods/api/`, `src/modules/foods/composables/` |
| Domain — Brands | `src/modules/brands/api/`, `src/modules/brands/composables/` |
| Domain — Nutrients | `src/modules/nutrients/api/`, `src/modules/nutrients/composables/` |
| Domain — Units | `src/modules/units/api/`, `src/modules/units/composables/` |
| Domain — Dishes | `src/modules/dishes/api/`, `src/modules/dishes/composables/`, `src/core/composables/useDishCalculator.ts` |
| UI layer | `src/views/`, `src/components/` |

## Stato dei rischi

| ID | Area | Evidenza | Impatto | Priorità | Stato |
|----|------|----------|---------|----------|-------|
| FW-01 | Async state duplicato | `useAsyncState` con `pendingCount`, `errorMessage`, `run()` — usato in tutti i composable | Medio | P1 | ✅ Risolto |
| FW-02 | Error observability | `ApiError` tipizzato con `status`, `title`, `detail` — propagato da interceptor | Medio | P1 | ✅ Risolto |
| FW-03 | Re-fetch su navigazione | Cache in-memory 60s con `load(force?)` in tutti i composable | Medio | P1 | ✅ Risolto |
| FW-04 | Reactive churn forms | Watcher deep su props in `FoodDetail.vue` e `FoodNutrientInput.vue` | Medio-Alto | P2 | ⚠️ Aperto |
| FW-05 | Dead interaction | `select` rimosso da `FoodsList.vue` — evento morto eliminato | Basso-Medio | P2 | ✅ Risolto |
| FW-07 | Scaffold starter | `App.vue` e shell puliti. `HomeView.vue` e `TheWelcome.vue` da verificare | Medio | P2 | ⚠️ Parziale |
| FW-08 | Naming/boundaries | Interfacce in `Interfaces/foods/` (kebab) vs `Interfaces/Nutrients/` (PascalCase) | Basso | P3 | ⚠️ Aperto |
| FW-09 | Test coverage | Nessun test frontend presente | Alto | P2 | ⚠️ Aperto |

## Funzionalità aggiunte

| ID | Feature | Descrizione | Stato |
|----|---------|-------------|-------|
| FW-10 | Ricerca e ordinamento liste | `useTableSearch` composable in `core/composables/`. Ricerca per nome (case-insensitive) e sort per colonna su Brands, Foods, Nutrients, Units | ✅ Completato |
| FW-11 | Quick-add Marca e UdM nel form Alimento | `BrandQuickAddModal.vue` e `UnitQuickAddModal.vue` — bottone "Nuovo" accanto ai dropdown nel form alimento | ✅ Completato |
| FW-12 | Dominio Piatti | `DishesView.vue`, `DishBuilder.vue`, `DishIngredientList.vue`, `DishNutritionPreview.vue`. Composable `useDishes` + `useDishCalculator`. Calcolo nutrienti proporzionale normalizzato a 100g | ✅ Completato |
| FW-13 | Marca nel form piatti | `DishIngredientList.vue`: autocomplete a due righe (nome + marca in grigio), colonna tabella `Nome (Marca)`. Reset combo via `nextTick` dopo selezione | ✅ Completato |
| FW-14 | Ordinamento nutrienti centralizzato | `sortNutrients.ts` in `core/utils/`. Ordine: `positionOrder` ASC (0 = non classificato → in fondo), poi `name` alfabetico. Applicato in `FoodDetail.vue` e `useDishCalculator.ts` | ✅ Completato |

## Opportunità di miglioramento residue

| Area | Problema attuale | Pattern suggerito | Beneficio atteso | Complessità |
|------|------------------|-------------------|------------------|-------------|
| Form reactivity | Watcher deep su full object in `FoodDetail.vue` | Watch su singoli valori o computed mirati | Meno update reattivi | M |
| ~~Dead event~~ | ~~`select` emesso ma non consumato~~ | ~~Collegare o rimuovere~~ | ~~Contratto eventi pulito~~ | ✅ Risolto |
| Scaffold | `HomeView.vue`, `TheWelcome.vue` ancora nel progetto | Rimuovere o sostituire | Meno rumore nel repository | S |
| Naming | `Interfaces/foods/` (kebab) vs `Interfaces/Nutrients/` (PascalCase) | Convergere su PascalCase gradualmente | Import/DX coerenti | M |
| Test | Nessun test su composable/component | Unit test composable + interaction test componenti | Confidenza nei refactor | M |

## Anti-pattern risolti

| Anti-pattern | Risolto con |
|--------------|-------------|
| Logica async/error duplicata in ogni composable | `useAsyncState` condiviso (`pendingCount`, `errorMessage`, `run()`) |
| Errori Axios non tipizzati, perdita status HTTP | `ApiError` con `status`, `title`, `detail` — costruito nell'interceptor |
| Refetch ad ogni navigazione | Cache in-memory 60s con TTL e invalidazione esplicita `force=true` |
| Risposta 409 generica uguale per tutti i conflitti | Messaggio specifico per ogni tipo di conflitto (nome duplicato, abbreviazione duplicata, FK in uso) |
| Scaffold starter nella shell | `App.vue` pulito, menu di navigazione funzionale |
| Nessuna ricerca/sort nelle tabelle | `useTableSearch` composable + `sorter` su tutte le colonne |
| Evento `select` emesso senza consumer | Rimosso da `FoodsList.vue` |
| Ordinamento nutrienti non deterministico | `sortNutrients` utility condivisa: `positionOrder` → alfabetico. Usata in `FoodDetail.vue` e `useDishCalculator.ts` |
| Combo autocomplete non si resettava dopo selezione | `nextTick` in `onFoodSelect` — NAutoComplete aggiorna il model dopo `@select` |

## Anti-pattern ancora presenti

- Watcher deep su props object in `FoodDetail.vue` e `FoodNutrientInput.vue`.
- `HomeView.vue` e `TheWelcome.vue` residui del template Vue starter.
- Naming `Interfaces/foods/` (kebab) non allineato agli altri moduli (PascalCase).

---
*Ultima revisione: 2026-03-25 | Focus: Frontend WebVue*
