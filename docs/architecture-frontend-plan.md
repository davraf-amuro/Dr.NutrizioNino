# Frontend Architecture — Piano interventi

## Obiettivi

- Risolvere il watcher deep nei form Foods.
- Rimuovere i residui scaffold starter.
- Convergere il naming delle interfacce su PascalCase.
- Introdurre test su composable e componenti critici.

---

## Backlog

| ID | Intervento | Effort | Stato |
|----|------------|--------|-------|
| FW-01 | `useAsyncState` condiviso | S | ✅ Completato |
| FW-02 | `ApiError` tipizzato con interceptor | S | ✅ Completato |
| FW-03 | Cache in-memory 60s per dominio | M | ✅ Completato |
| FW-04 | Refactor reattività form Foods — watch mirati | M | ⚠️ Da fare |
| FW-05 | Evento `select` morto rimosso da `FoodsList` | S | ✅ Completato |
| FW-06 | Tutti i form usano Naive UI | S | ✅ Completato |
| FW-07 | Rimuovere scaffold starter (`HomeView`, `TheWelcome`) | S | ⚠️ Parziale |
| FW-08 | Naming `Interfaces/` uniformato su PascalCase | M | ⚠️ Da fare |
| FW-09 | Test frontend — composable + componenti | M | ⚠️ Da fare |
| FW-10 | Ricerca e sort tabelle (`useTableSearch`) | S | ✅ Completato |
| FW-11 | Quick-add Marca / UdM / Supermercato nel form Alimento | M | ✅ Completato |
| FW-12 | Dominio Piatti completo | M | ✅ Completato |
| FW-14 | Ordinamento nutrienti centralizzato (`sortNutrients`) | S | ✅ Completato |
| FW-15 | Dominio Supermercati completo | M | ✅ Completato |
| FW-17 | Dettaglio piatto read-only | S | ✅ Completato |
| FW-18 | Auth migrata a localStorage + header Bearer | S | ✅ Completato |
| FW-19 | Nutrienti — default grammi + campi allineati | M | ✅ Completato |
| FW-20 | Dashboard — colonne Kcal e Barcode rimosse | S | ✅ Completato |
| FW-21 | Dominio Categorie completo + quick-add nel form | M | ✅ Completato |
| FW-22 | Admin utenti (CRUD UI + guard rotta) | M | ✅ Completato |
| FW-23 | App shell — menu condizionale admin + username header | S | ✅ Completato |
| FW-24 | Confronto alimenti — selezione multipla + modale griglia | M | ✅ Completato |
| FW-25 | Dropdown filtrabili + ordine alfabetico | S | ✅ Completato |
| FW-26 | Menu "Configurazione" raggruppato — dropdown con 5 anagrafiche | S | ✅ Completato |
| FW-27 | Bottoni azioni uniformati (secondary/tertiary) in DishesList | S | ✅ Completato |
| FW-28 | Peso totale piatto in footer griglia ingredienti | S | ✅ Completato |
| FW-29 | Dashboard piatti — colonna Kcal rinominata, Qta aggiunta | S | ✅ Completato |
| FW-30 | Calorie rimossa da FoodDto/DishDetailDto; mantenuta in FoodDashboardDto | S | ✅ Completato |

---

## Prossime priorità

### Quick Wins

- **FW-07**: verificare se `HomeView.vue` e `TheWelcome.vue` sono raggiungibili dall'utente; rimuoverli se inutilizzati.

### Mid-term

- **FW-04**: refactor `FoodDetail.vue` — sostituire il watcher deep sull'object prop con watch mirati sui singoli campi o computed.
- **FW-08**: rinominare `Interfaces/foods/` → `Interfaces/Foods/` e aggiornare gli import.
- **FW-09**: aggiungere test per `useNutrients`, `useUnits`, `useBrands` — scenari: caricamento, cache, errori.

---

## KPI

| KPI | Target | Stato |
|-----|--------|-------|
| Blocchi async/error duplicati | 0 | ✅ |
| Refetch per revisit stessa sessione | 0 (cache 60s) | ✅ |
| Evento emesso senza consumer | 0 | ✅ |
| Ricerca e sort su tutte le tabelle | ✅ | ✅ |
| Watcher deep su object props | 0 | ⚠️ FW-04 aperto |
| Scaffold starter nel flusso utente | 0 | ⚠️ FW-07 parziale |
| Cartelle `Interfaces/` naming PascalCase | 100% | ⚠️ FW-08 aperto |
| Test composable | ≥ 8 | ⚠️ FW-09 aperto |

---

*Ultima revisione: 2026-04-04 — modello `claude-sonnet-4-6`*
