# Frontend Architecture Findings

## Executive Summary

Il frontend segue un'architettura feature-oriented (`modules/*/api` + `modules/*/composables`) coerente su tutti i domini (Foods, Brands, Nutrients, Units).

Le aree risolte dall'analisi iniziale (2026-03-02): `useAsyncState` condiviso, `ApiError` tipizzato, cache in-memory per dominio, pulizia scaffold dalla shell.

Rimangono aperti: reattività form Foods (watcher deep), test frontend, convergenza naming.

## Architecture Map

| Layer | File/Cartella |
|-------|---------------|
| App shell + routing | `src/App.vue`, `src/router/index.ts` |
| HTTP core | `src/core/http/apiClient.ts`, `src/core/http/ApiError.ts` |
| Async state | `src/core/composables/useAsyncState.ts` |
| Domain — Foods | `src/modules/foods/api/`, `src/modules/foods/composables/` |
| Domain — Brands | `src/modules/brands/api/`, `src/modules/brands/composables/` |
| Domain — Nutrients | `src/modules/nutrients/api/`, `src/modules/nutrients/composables/` |
| Domain — Units | `src/modules/units/api/`, `src/modules/units/composables/` |
| UI layer | `src/views/`, `src/components/` |

## Stato dei rischi

| ID | Area | Evidenza | Impatto | Priorità | Stato |
|----|------|----------|---------|----------|-------|
| FW-01 | Async state duplicato | `useAsyncState` con `pendingCount`, `errorMessage`, `run()` — usato in tutti i composable | Medio | P1 | ✅ Risolto |
| FW-02 | Error observability | `ApiError` tipizzato con `status`, `title`, `detail` — propagato da interceptor | Medio | P1 | ✅ Risolto |
| FW-03 | Re-fetch su navigazione | Cache in-memory 60s con `load(force?)` in tutti i composable | Medio | P1 | ✅ Risolto |
| FW-04 | Reactive churn forms | Watcher deep su props in `FoodDetail.vue` e `FoodNutrientInput.vue` | Medio-Alto | P2 | ⚠️ Aperto |
| FW-05 | Dead interaction | `select` emesso in `FoodsList.vue` ma non gestito in `FoodsView.vue` | Basso-Medio | P2 | ⚠️ Aperto |
| FW-07 | Scaffold starter | `App.vue` e shell puliti. `HomeView.vue` e `TheWelcome.vue` da verificare | Medio | P2 | ⚠️ Parziale |
| FW-08 | Naming/boundaries | Interfacce in `Interfaces/foods/` (kebab) vs `Interfaces/Nutrients/` (PascalCase) | Basso | P3 | ⚠️ Aperto |
| FW-09 | Test coverage | Nessun test frontend presente | Alto | P2 | ⚠️ Aperto |

## Opportunità di miglioramento residue

| Area | Problema attuale | Pattern suggerito | Beneficio atteso | Complessità |
|------|------------------|-------------------|------------------|-------------|
| Form reactivity | Watcher deep su full object in `FoodDetail.vue` | Watch su singoli valori o computed mirati | Meno update reattivi | M |
| Dead event | `select` emesso ma non consumato in `FoodsView.vue` | Collegare o rimuovere | Contratto eventi pulito | S |
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

## Anti-pattern ancora presenti

- Watcher deep su props object in `FoodDetail.vue` e `FoodNutrientInput.vue`.
- Evento `select` emesso da `FoodsList.vue` senza consumer in `FoodsView.vue`.
- `HomeView.vue` e `TheWelcome.vue` residui del template Vue starter.
- Naming `Interfaces/foods/` (kebab) non allineato agli altri moduli (PascalCase).

---
*Ultima revisione: 2026-03-18 | Focus: Frontend WebVue*
