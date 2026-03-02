````prompt
# Prompt: Architecture Fullstack WebVue Planner (AI Agent)

Analizza end-to-end backend + WebVue e pianifica interventi architetturali per migliorare riusabilita, performance e coerenza contrattuale API/UI.

## Scope
- Backend Minimal API + frontend WebVue
- Focus sui flussi principali dati (API contract -> DTO -> UI state -> rendering)

## Vincoli obbligatori
- Segui:
  - `.github/copilot-instructions.md`
  - `.github/instructions/minimal-api-architecture.instructions.md`
  - `.github/instructions/database-provider.instructions.md`
- Evita pattern backend vietati (MVC Controllers, IRepository, AutoMapper, MediatR)

## Obiettivi analisi
1. Coerenza contratti API (status code, payload, naming, versioning)
2. Riuso DTO/mapper/trasformazioni tra layer
3. Frontend data flow (fetching, caching, gestione errori, stato)
4. Performance E2E (chatty API, payload eccessivi, rendering inefficiente)
5. Operativita: logging correlabile e troubleshooting cross-layer

## Output
- Crea/aggiorna `docs/architecture-fullstack-findings.md`
- Crea/aggiorna `docs/architecture-fullstack-plan.md`

## Tabelle obbligatorie

### Gap architetturali E2E
| Flusso | Evidenza | Impatto utente | Layer coinvolti | Priorita |
|--------|----------|----------------|-----------------|----------|

### Backlog interventi E2E
| ID | Intervento | Pattern/Approccio | Layer | Effort (S/M/L) | Impatto (1-5) | KPI | Rischi |
|----|------------|-------------------|-------|----------------|---------------|-----|-------|

## KPI suggeriti (se disponibili)
- P95 tempo risposta API chiave
- Tempo interazione UI (load/list/detail)
- Errore lato client per route/feature
- Dimensione payload media endpoint principali

## Regole
- Basati solo su evidenze del repository
- Se mancano metriche FE/BE, dichiaralo e proponi instrumentation minima
- Proponi rollout incrementale senza big-bang

## Risposta del prompt
Rispondi solo con:
1) file creati/aggiornati
2) top 5 interventi fullstack prioritari
3) gap informativi bloccanti

*Template v1.0 - .NET 10 - Fullstack Focus* - Last Update 2026-03-02 00:00
````
