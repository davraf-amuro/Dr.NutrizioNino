# Frontend Architecture Intervention Plan

## Obiettivi misurabili
- Ridurre di almeno il 70% la duplicazione di logica async/error nei composable.
- Ridurre di almeno il 40% i refetch su revisit delle route nella stessa sessione.
- Ridurre di almeno il 30% gli update reattivi nel flow di editing food.
- Portare al 100% la coerenza dei controlli UI nei target view (Naive UI vs native).
- Introdurre baseline test frontend (almeno 8 test mirati composable/component).

## Backlog prioritizzato
| ID | Intervento | Pattern/Approach | Area | Effort (S/M/L) | Impatto (1-5) | Rischio | Dipendenze | KPI |
|---|---|---|---|---|---:|---|---|---|
| FW-01 | Introdurre composable async-state condiviso | `useAsyncState` con `pendingCount`, `error`, `run()` | State/Data flow | S | 5 | Low | Nessuna | blocchi duplicati rimossi |
| FW-02 | Tipizzare il modello errore API | `ApiError` centralizzato da interceptor con status/detail | API/Error handling | S | 5 | Low | FW-01 opzionale | % errori con gestione status-aware |
| FW-03 | Cache per dominio con invalidazione esplicita | cache in-memory nei composable (`load(force?)`) | Performance/Data | M | 4 | Medium | FW-01 | API calls per revisit/session |
| FW-04 | Refactor reattività form food | eliminare watcher deep, usare watch/computed mirati | Rendering/Perf | M | 4 | Medium | FW-01 | update reattivi per edit flow |
| FW-05 | Completare/rimuovere azione `select` in foods list | collegare `@select` o eliminare azione morta | Component responsibilities | S | 3 | Low | Nessuna | dead event count = 0 |
| FW-06 | Standardizzare primitive UI nei view target | usare componenti Naive UI coerenti | UI consistency | S | 2 | Low | Nessuna | occorrenze controlli misti |
| FW-07 | Rimuovere scaffold starter da shell/home/about | sostituire con shell prodotto | Maintainability | S | 3 | Low | Nessuna | riferimenti scaffold = 0 |
| FW-08 | Migrazione naming consistency progressiva | convergenza casing/modules su aree toccate | DX/Maintainability | M | 3 | Medium | FW-07 | issue import/casing |
| FW-09 | Aggiungere test frontend mirati | unit composable + interaction component | Quality gate | M | 4 | Medium | FW-01, FW-04 | test count, regressioni intercettate |

## Piano per fasi

### Quick Wins (1-2 sprint)
- FW-01 `useAsyncState` condiviso.
- FW-02 `ApiError` tipizzato.
- FW-05 completare/rimuovere `select` morto.
- FW-06 standardizzazione minima controlli UI.

### Mid-term (2-4 sprint)
- FW-03 cache per dominio + invalidazione.
- FW-04 refactor reattività form food.
- FW-09 baseline test frontend.

### Strategic (continuo)
- FW-07 pulizia completa scaffold starter.
- FW-08 convergenza naming/casing graduale.

## KPI tecnici
- Numero blocchi duplicati async/error.
- Numero richieste per revisit route nella stessa sessione.
- Tempo medio/TTI delle view Foods/Brands dopo primo load.
- Numero update reattivi nel flow edit (baseline vs target).
- Copertura test e regressioni intercettate su composable/component.

## Criteri di accettazione
- Nessun composable critico mantiene wrapper async/error ad-hoc duplicato.
- Gli errori API esposti ai component includono almeno `message` + `status`.
- Revisit Foods/Brands usa cache salvo `refresh` esplicito.
- Nessun watcher deep su full object props nei form core.
- Nessun evento emesso resta senza consumer nel parent.
- Nessun componente scaffold rimane nel flusso utente primario.

## Top 5 interventi
- FW-01 shared async-state composable.
- FW-02 typed API error envelope.
- FW-03 composable cache con invalidazione.
- FW-04 food form reactivity refactor.
- FW-09 test harness minimale per composable/component.

---
*Piano generato il: 2026-03-02 | Focus: Frontend WebVue | LLM: GitHub Copilot*