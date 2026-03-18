# Frontend Architecture Intervention Plan

## Obiettivi

- Eliminare watcher deep nei form Foods.
- Completare o rimuovere l'evento `select` morto in FoodsList.
- Rimuovere i residui scaffold starter.
- Uniformare naming delle interfacce.
- Introdurre baseline test su composable e componenti critici.

## Backlog interventi

| ID | Intervento | Pattern/Approccio | Effort | Impatto (1-5) | Stato |
|----|------------|-------------------|--------|---------------|-------|
| FW-01 | `useAsyncState` condiviso | `pendingCount`, `errorMessage`, `run()` | S | 5 | ✅ Completato |
| FW-02 | `ApiError` tipizzato | Interceptor con `status`, `title`, `detail` | S | 5 | ✅ Completato |
| FW-03 | Cache per dominio | Cache in-memory 60s con `load(force?)` | M | 4 | ✅ Completato |
| FW-04 | Refactor reattività form Foods | Watch su singoli valori, no watcher deep | M | 4 | ⚠️ Da fare |
| FW-05 | Completare/rimuovere `select` in Foods | Collegare a funzione di dettaglio o rimuovere | S | 3 | ⚠️ Da fare |
| FW-06 | Standardizzare controlli UI | Tutti i form usano Naive UI | S | 2 | ✅ Completato (nuovi moduli) |
| FW-07 | Rimuovere scaffold starter | Eliminare `HomeView.vue`, `TheWelcome.vue` | S | 3 | ⚠️ Parziale |
| FW-08 | Naming consistency | Convergere `Interfaces/foods/` → `Interfaces/Foods/` | M | 3 | ⚠️ Da fare |
| FW-09 | Test frontend | Unit composable + interaction componenti | M | 4 | ⚠️ Da fare |

## Piano per fasi aggiornato

### Quick Wins (prossimo sprint)

- **FW-05**: verificare se `select` in `FoodsList.vue` serve — collegarlo o eliminarlo
- **FW-07**: rimuovere `HomeView.vue`, `TheWelcome.vue` e relativo routing se non usati

### Mid-term (1-2 sprint)

- **FW-04**: refactor `FoodDetail.vue` e `FoodNutrientInput.vue` — sostituire watcher deep con watch mirati su valori singoli
- **FW-08**: rinominare `Interfaces/foods/` → `Interfaces/Foods/` e aggiornare gli import
- **FW-09**: aggiungere test per almeno `useNutrients`, `useUnits`, `useBrands` (caricamento, cache, errori)

### Strategic

- Convergenza naming completa su tutti i moduli.
- Test harness per i componenti Forms critici.

## KPI tecnici

| KPI | Target | Stato |
|-----|--------|-------|
| Blocchi async/error duplicati | 0 | ✅ Risolto |
| Refetch per revisit stessa sessione | 0 (cache 60s) | ✅ Risolto |
| Watcher deep su object props | 0 | ⚠️ FW-04 pending |
| Scaffold starter nel flusso utente | 0 | ⚠️ FW-07 parziale |
| Test composable | ≥ 8 | ⚠️ FW-09 pending |
| Cartelle `Interfaces/` con naming coerente | 100% PascalCase | ⚠️ FW-08 pending |

## Criteri di accettazione aperti

- Nessun watcher deep su full object props nei form core.
- Nessun evento emesso resta senza consumer nel parent.
- Nessun componente scaffold rimane nel flusso utente primario.
- Tutte le cartelle `Interfaces/` usano naming PascalCase.
- Suite test copre almeno i composable principali con scenari di caricamento, cache e gestione errori.

---
*Ultima revisione: 2026-03-18 | Focus: Frontend WebVue*
