# Frontend Architecture Findings

## Executive Summary

Il frontend segue un'architettura feature-oriented: `modules/*/api` + `modules/*/composables` per ogni dominio, con un layer `core/` per le utility condivise.

I problemi principali di async duplicato, re-fetch su navigazione e eventi morti sono stati risolti nelle prime sessioni (2026-03-02 / 2026-03-26).

Feature aggiunte (2026-04-03): dominio Categorie completo, gestione utenti admin (CRUD UI), miglioramenti ai Nutrienti (default grammi, campi visibili sia in create che in edit), confronto alimenti con modale griglia, aggiornamento App shell (menu admin condizionale, indicatore utente).

Rimangono aperti: watcher deep in FoodDetail, test frontend, convergenza naming interfacce.

---

## Mappa architettura corrente

| Layer | File/Cartella |
|-------|---------------|
| App shell + routing | `src/App.vue`, `src/router/index.ts` |
| HTTP core | `src/core/http/apiClient.ts`, `src/core/http/ApiError.ts`, `src/core/http/tokenStorage.ts` |
| Async state + utility | `src/core/composables/useAsyncState.ts`, `src/core/composables/useTableSearch.ts` |
| Utility condivise | `src/core/utils/sortNutrients.ts` |
| Auth | `src/modules/auth/api/`, `src/modules/auth/composables/useAuth.ts` |
| Domain — Foods | `src/modules/foods/api/`, `src/modules/foods/composables/` |
| Domain — Brands | `src/modules/brands/api/`, `src/modules/brands/composables/` |
| Domain — Nutrients | `src/modules/nutrients/api/`, `src/modules/nutrients/composables/` |
| Domain — Units | `src/modules/units/api/`, `src/modules/units/composables/` |
| Domain — Dishes | `src/modules/dishes/api/`, `src/modules/dishes/composables/`, `src/core/composables/useDishCalculator.ts` |
| Domain — Supermarkets | `src/modules/supermarkets/api/`, `src/modules/supermarkets/composables/` |
| Domain — Categories | `src/modules/categories/api/`, `src/modules/categories/composables/` |
| Domain — Admin | `src/modules/admin/api/admin.api.ts`, `src/modules/admin/composables/useAdminUsers.ts` |
| UI layer | `src/views/`, `src/components/` (Admin, Brands, Categories, Dishes, Foods, Nutrients, Supermarkets, Units) |

---

## Stato dei rischi

| ID | Area | Evidenza | Stato |
|----|------|----------|-------|
| FW-01 | Async state duplicato | `useAsyncState` condiviso in tutti i composable | ✅ Risolto |
| FW-02 | Error observability | `ApiError` tipizzato con `status`, `title`, `detail` | ✅ Risolto |
| FW-03 | Re-fetch su navigazione | Cache in-memory 60s con `load(force?)` per tutti i domini | ✅ Risolto |
| FW-04 | Reactive churn forms | Watcher deep su props in `FoodDetail.vue` e `FoodNutrientInput.vue` | ⚠️ Aperto |
| FW-07 | Scaffold starter | `HomeView.vue`, `TheWelcome.vue` ancora nel progetto | ⚠️ Parziale |
| FW-08 | Naming consistency | `Interfaces/foods/` (kebab) vs `Interfaces/Nutrients/` (PascalCase) | ⚠️ Aperto |
| FW-09 | Test coverage | Nessun test frontend presente | ⚠️ Aperto |

---

## Funzionalità aggiunte

### Sessioni 2026-03-25 / 2026-03-26

| ID | Feature | Descrizione |
|----|---------|-------------|
| FW-10 | Ricerca e sort tabelle | `useTableSearch` composable. Ricerca per nome e sort per colonna su tutti i domini |
| FW-11 | Quick-add nel form Alimento | `BrandQuickAddModal`, `UnitQuickAddModal`, `SupermarketQuickAddModal` — modale inline con gestione 409 |
| FW-12 | Dominio Piatti | `DishesView`, `DishBuilder`, `DishIngredientList`, `DishNutritionPreview`. `useDishes` + `useDishCalculator` |
| FW-14 | Ordinamento nutrienti centralizzato | `sortNutrients.ts`: `positionOrder` ASC (0 in fondo) → alfabetico. Usato in FoodDetail, FoodCompareModal, useDishCalculator |
| FW-15 | Dominio Supermercati | `SupermarketsView`, CRUD completo. `FoodsList` con colonna tag supermercati |
| FW-17 | Dettaglio piatto read-only | `DishDetail.vue`: tabella ingredienti + preview nutrienti |

### Sessione 2026-04-03

| ID | Feature | Descrizione |
|----|---------|-------------|
| FW-18 | Autenticazione localStorage | Token JWT salvato in localStorage via `tokenStorage.ts`. Axios interceptor aggiunge `Authorization: Bearer`. `useAuth` valida la scadenza localmente prima di chiamare `/me` |
| FW-19 | Nutrienti — default grammi | `NutrientDetail.vue`: campi Unità, Ordine, Quantità visibili sia in create che in edit. Default unità = "g" (auto-selezionato quando disponibile). `CreateNutrientRequest` esteso con `defaultUnitOfMeasureId`, `positionOrder`, `defaultQuantity` |
| FW-20 | Dashboard — pulizia colonne | `FoodsList.vue`: colonna Kcal e colonna Barcode rimosse. Layout più compatto |
| FW-21 | Dominio Categorie | `CategoriesView`, `CategoriesList`, `CategoryDetail`, `CategoryQuickAddModal`. `useCategories` con cache 60s. Route `/categories`. `FoodDetail` con multi-select categorie e quick-add |
| FW-22 | Admin utenti | `AdminUsersView`, `UsersList`, `UserDetail`. `useAdminUsers` composable con create/edit/delete/changeRole. Route `/admin/users` con guard `requiresAdmin` |
| FW-23 | App shell aggiornata | Menu "Utenti" visibile solo agli Admin (condizionale su `isAdmin`). Username dell'utente corrente nell'header (da `useAuth().user`) |
| FW-24 | Confronto alimenti | `FoodsList`: colonna di selezione multipla + bottone "Confronta" (attivo con ≥2 selezioni). `FoodCompareModal`: tabella con colonne=alimenti, righe=nutrienti ordinati per `positionOrder`. ▲ verde per massimo, ▼ rosso per minimo per riga. Abbreviazione UdM accanto al nome nutriente |
| FW-25 | Dropdown migliorati | `FoodDetail`: tutti i select con `filterable` (ricerca inline). Opzioni ordinate alfabeticamente con `localeCompare('it')`. Campo nome con placeholder |

---

## Anti-pattern risolti

| Anti-pattern | Risolto con |
|--------------|-------------|
| Logica async/error duplicata | `useAsyncState` condiviso |
| Errori Axios non tipizzati | `ApiError` con `status`, `title`, `detail` |
| Refetch ad ogni navigazione | Cache in-memory 60s con TTL |
| Ordinamento nutrienti non deterministico | `sortNutrients` — `positionOrder` → alfabetico |
| Evento `select` senza consumer | Rimosso da `FoodsList.vue` |
| Nessuna ricerca/sort nelle tabelle | `useTableSearch` + `sorter` su tutte le colonne |
| Quick-add lookup assente nel form Alimento | Modal inline per Brand, UdM, Supermercato, Categoria |
| Token JWT via cookie cross-origin problematico | Migrazione a localStorage + Authorization header |
| Colonne ridondanti in dashboard | Barcode e Kcal rimossi da `FoodsList` |
| Menu admin visibile a tutti | Condizionale su `isAdmin` computed |

## Anti-pattern ancora presenti

- Watcher deep su props object in `FoodDetail.vue` e `FoodNutrientInput.vue`.
- `HomeView.vue` e `TheWelcome.vue` residui del template Vue starter.
- `Interfaces/foods/` in kebab-case vs altri moduli in PascalCase.
- Nessun test frontend (composable o componenti).

---

*Ultima revisione: 2026-04-03 — modello `claude-sonnet-4-6`*
