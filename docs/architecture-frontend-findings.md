# Frontend Architecture Findings

## Executive Summary
- Il frontend segue una base feature-oriented (`modules/*/api` + `modules/*/composables`) e oggi è navigabile.
- Lo stato è locale ai composable/view: senza store condiviso i dati vengono rifetchati al remount delle route.
- La gestione errori Axios centralizzata perde contesto HTTP (status/detail), limitando UX e diagnosi.
- I principali rischi performance sono watcher deep e watcher per-item nel flusso form food.
- Sono presenti residui del template Vue starter in shell/home.
- Ci sono quick wins incrementali: async-state condiviso, envelope errore tipizzato, cache di dominio e tightening dei watcher.

## Architecture Map
- App shell e routing:
  - [src/Dr.NutrizioNino.WebVue/src/main.ts](src/Dr.NutrizioNino.WebVue/src/main.ts)
  - [src/Dr.NutrizioNino.WebVue/src/App.vue](src/Dr.NutrizioNino.WebVue/src/App.vue)
  - [src/Dr.NutrizioNino.WebVue/src/router/index.ts](src/Dr.NutrizioNino.WebVue/src/router/index.ts)
- HTTP core:
  - [src/Dr.NutrizioNino.WebVue/src/core/http/apiClient.ts](src/Dr.NutrizioNino.WebVue/src/core/http/apiClient.ts)
- Domain modules:
  - Brands API: [src/Dr.NutrizioNino.WebVue/src/modules/brands/api/brands.api.ts](src/Dr.NutrizioNino.WebVue/src/modules/brands/api/brands.api.ts)
  - Brands composable: [src/Dr.NutrizioNino.WebVue/src/modules/brands/composables/useBrands.ts](src/Dr.NutrizioNino.WebVue/src/modules/brands/composables/useBrands.ts)
  - Foods API: [src/Dr.NutrizioNino.WebVue/src/modules/foods/api/foods.api.ts](src/Dr.NutrizioNino.WebVue/src/modules/foods/api/foods.api.ts)
  - Foods composable: [src/Dr.NutrizioNino.WebVue/src/modules/foods/composables/useFoods.ts](src/Dr.NutrizioNino.WebVue/src/modules/foods/composables/useFoods.ts)
  - Units API: [src/Dr.NutrizioNino.WebVue/src/modules/units/api/units.api.ts](src/Dr.NutrizioNino.WebVue/src/modules/units/api/units.api.ts)
- UI layer:
  - [src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue](src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue)
  - [src/Dr.NutrizioNino.WebVue/src/views/BrandsView.vue](src/Dr.NutrizioNino.WebVue/src/views/BrandsView.vue)
  - [src/Dr.NutrizioNino.WebVue/src/components/Foods](src/Dr.NutrizioNino.WebVue/src/components/Foods)
  - [src/Dr.NutrizioNino.WebVue/src/components/Brands](src/Dr.NutrizioNino.WebVue/src/components/Brands)

## Risk Table
| Area | Evidence | Impact | Probability | Priority |
|---|---|---:|---:|---:|
| Async state race | `isLoading` condiviso su chiamate parallele in [src/Dr.NutrizioNino.WebVue/src/modules/foods/composables/useFoods.ts](src/Dr.NutrizioNino.WebVue/src/modules/foods/composables/useFoods.ts#L19-L43) | Medium | High | P1 |
| Error observability | Interceptor converte in `Error(message)` in [src/Dr.NutrizioNino.WebVue/src/core/http/apiClient.ts](src/Dr.NutrizioNino.WebVue/src/core/http/apiClient.ts#L13-L18) | Medium | High | P1 |
| Re-fetch on navigation | `onMounted` ricarica sempre in [src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue](src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue#L23-L25) e [src/Dr.NutrizioNino.WebVue/src/views/BrandsView.vue](src/Dr.NutrizioNino.WebVue/src/views/BrandsView.vue#L21-L23) | Medium | High | P1 |
| Reactive churn forms | Watcher deep/clone in [src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue](src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue#L110-L156) e watch per-item in [src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodNutrientInput.vue](src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodNutrientInput.vue#L53-L68) | Medium-High | Medium | P2 |
| Dead interaction | `select` emesso ma non gestito in [src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodsList.vue](src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodsList.vue#L15-L34) e [src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue](src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue#L34) | Low-Medium | High | P2 |
| Maintainability drift | Residui starter in [src/Dr.NutrizioNino.WebVue/src/App.vue](src/Dr.NutrizioNino.WebVue/src/App.vue#L3-L17), [src/Dr.NutrizioNino.WebVue/src/views/HomeView.vue](src/Dr.NutrizioNino.WebVue/src/views/HomeView.vue#L1-L8), [src/Dr.NutrizioNino.WebVue/src/components/TheWelcome.vue](src/Dr.NutrizioNino.WebVue/src/components/TheWelcome.vue) | Medium | High | P2 |

## Opportunities Table
| Area | Current issue | Suggested pattern | Expected benefit | Complexity |
|---|---|---|---|---|
| Async handling | Duplicazioni `withErrorHandling` | `useAsyncState` condiviso | Semantica loading/error uniforme | S |
| API contracts | Errori non tipizzati | `ApiError` tipizzato con status/detail | UX più ricca e debugging migliore | S |
| Data lifetime | Refetch su remount | Cache in-memory per dominio + `refresh()` | Meno roundtrip e navigazione più veloce | M |
| Form reactivity | Watcher deep su modelli mutabili | Watch/computed mirati + update espliciti | Riduzione overhead reattivo | M |
| UI consistency | Mix controlli nativi + Naive UI | Standardizzare componenti UI | UX prevedibile e theming più semplice | S |
| Naming/boundaries | Naming/casing disomogeneo | Convenzione progressiva sui moduli toccati | DX/import consistency | M |

## Anti-patterns (evidenze)
- Watcher deep su full object props: [src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue](src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue#L134-L148), [src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodNutrientInput.vue](src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodNutrientInput.vue#L53-L59).
- `isLoading` unico per operazioni concorrenti in [src/Dr.NutrizioNino.WebVue/src/modules/foods/composables/useFoods.ts](src/Dr.NutrizioNino.WebVue/src/modules/foods/composables/useFoods.ts#L19-L43).
- Contratto eventi non consumato (`select`) in [src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodsList.vue](src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodsList.vue#L15-L34) e [src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue](src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue#L34).
- Layer API frammentato per shape/return in [src/Dr.NutrizioNino.WebVue/src/modules/foods/api/foods.api.ts](src/Dr.NutrizioNino.WebVue/src/modules/foods/api/foods.api.ts) e [src/Dr.NutrizioNino.WebVue/src/modules/brands/api/brands.api.ts](src/Dr.NutrizioNino.WebVue/src/modules/brands/api/brands.api.ts).
- Residui scaffold nel flusso primario in [src/Dr.NutrizioNino.WebVue/src/App.vue](src/Dr.NutrizioNino.WebVue/src/App.vue) e [src/Dr.NutrizioNino.WebVue/src/components/TheWelcome.vue](src/Dr.NutrizioNino.WebVue/src/components/TheWelcome.vue).

---
*Documento generato il: 2026-03-02 | Focus: Frontend WebVue | LLM: GitHub Copilot*
