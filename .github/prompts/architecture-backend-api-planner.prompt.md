````prompt
# Prompt: Architecture Backend API Planner (AI Agent)

Analizza esclusivamente il backend API della solution e pianifica interventi incrementali per migliorare riusabilita, performance e manutenibilita.

## Scope
- Solo progetto/i backend .NET Minimal API
- Escludi frontend WebVue salvo impatti diretti sulle API

## Vincoli obbligatori
- Segui:
  - `.github/copilot-instructions.md`
  - `.github/instructions/minimal-api-architecture.instructions.md`
  - `.github/instructions/database-provider.instructions.md`
- Non proporre: MVC Controllers, IRepository, AutoMapper, MediatR

## Obiettivi analisi
1. Design endpoint (versioning, route group, metadata OpenAPI)
2. Boundary API/Infrastructure/DTO e mapping
3. Accesso dati, query patterns, filtri, projection riusabili
4. Cross-cutting: logging, validation, ProblemDetails, sicurezza
5. Performance hotspot backend (I/O, allocazioni, serializzazione)

## Output
- Crea/aggiorna `docs/architecture-backend-findings.md`
- Crea/aggiorna `docs/architecture-backend-plan.md`

## Backlog tabella obbligatoria
| ID | Intervento | Pattern/Approccio | Area API | Effort (S/M/L) | Impatto (1-5) | Rischio | Dipendenze | KPI |
|----|------------|-------------------|----------|----------------|---------------|--------|------------|-----|

## Regole
- Solo evidenze reali dal codice
- Nessun dato inventato
- Ogni proposta include trade-off
- Evidenzia Quick Wins prima di Mid-term e Strategic

## Risposta del prompt
Rispondi solo con:
1) file creati/aggiornati
2) top 5 interventi backend prioritari
3) gap informativi bloccanti

*Template v1.0 - .NET 10 - Backend Focus* - Last Update 2026-03-02 00:00
````