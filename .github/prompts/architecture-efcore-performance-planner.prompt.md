````prompt
# Prompt: Architecture EF Core Performance Planner (AI Agent)

Analizza il layer dati e pianifica interventi mirati alle performance EF Core e alla riusabilita delle query/projection.

## Scope
- Focus primario: DbContext, provider, query LINQ, filtri, projection, mapping DTO
- Includi endpoint solo se impattano le query o il carico DB

## Vincoli obbligatori
- Segui:
  - `.github/copilot-instructions.md`
  - `.github/instructions/minimal-api-architecture.instructions.md`
  - `.github/instructions/database-provider.instructions.md`
- Mantieni compatibilita con Minimal API e pattern correnti del progetto

## Checklist tecnica di analisi
1. Uso di `AsNoTracking` per read-only
2. Query materialization e uso corretto di `CancellationToken`
3. Projection `Expression` riusabili e mapping manuale efficiente
4. N+1 query, include superflue, split query/carteziani
5. Filtri, ordinamenti, paginazione e possibilita di indici DB
6. Logging query e osservabilita minima necessaria

## Output
- Crea/aggiorna `docs/architecture-efcore-findings.md`
- Crea/aggiorna `docs/architecture-efcore-performance-plan.md`

## Tabelle obbligatorie

### Findings performance
| Area | Evidenza | Impatto stimato | Rischio regressione | Priorita |
|------|----------|-----------------|---------------------|----------|

### Backlog interventi
| ID | Intervento | Pattern/Approccio | Effort (S/M/L) | Impatto (1-5) | KPI performance | Note rollout |
|----|------------|-------------------|----------------|---------------|-----------------|-------------|

## KPI suggeriti (se misurabili)
- P95 latenza endpoint data-heavy
- Numero query per request
- Tempo medio query principali
- Percentuale query no-tracking su read endpoint

## Regole
- Non inventare benchmark o numeri
- Se non ci sono metriche, proponi piano di misurazione
- Interventi piccoli, verificabili, reversibili

## Risposta del prompt
Rispondi solo con:
1) file creati/aggiornati
2) top 5 interventi EF Core prioritari
3) gap informativi bloccanti

*Template v1.0 - .NET 10 - EF Core Performance Focus* - Last Update 2026-03-02 00:00
````
