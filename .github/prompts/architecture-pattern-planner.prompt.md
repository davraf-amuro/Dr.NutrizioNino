````prompt
# Prompt: Architecture Pattern Planner (AI Agent)

Analizza l'architettura dell'intera solution e pianifica interventi progressivi per migliorare riusabilita, performance e manutenibilita.

## Obiettivo
- Fare un assessment tecnico della solution (backend, frontend, test, infrastruttura)
- Individuare colli di bottiglia e duplicazioni
- Proporre i pattern piu adatti al contesto reale del progetto
- Generare un piano eseguibile per iterazioni con priorita chiare

## Vincoli obbligatori progetto
- Stack backend: .NET 10 Minimal API
- Segui sempre le istruzioni in `.github/copilot-instructions.md`
- Rispetta i file:
  - `.github/instructions/minimal-api-architecture.instructions.md`
  - `.github/instructions/database-provider.instructions.md`
- Non proporre o introdurre pattern vietati:
  - MVC Controllers
  - IRepository pattern
  - AutoMapper
  - MediatR

## Ambito analisi
1. Struttura solution e dipendenze tra progetti
2. Qualita dei boundary (API, Application, Infrastructure, DTO, mapping)
3. Accesso dati (query, tracking, filtri, projection, cancellazione)
4. Endpoint design (versioning, route group, metadata OpenAPI, error handling)
5. Cross-cutting concerns (logging, validazione, sicurezza, configurazione)
6. Performance hotspots (I/O, allocazioni, round-trip DB, serializzazione)
7. Riusabilita (duplicazioni, utility condivise, convenzioni)

## Output richiesto
- Crea/aggiorna `docs/architecture-intervention-plan.md`
- Crea/aggiorna `docs/architecture-findings.md`

## Formato output 1: architecture-findings.md
Sezioni minime:
1. Executive Summary (max 10 righe)
2. Mappa architettura corrente
3. Rischi principali (tabella)
4. Opportunita di miglioramento (tabella)
5. Anti-pattern trovati (solo evidenze reali)

### Tabelle richieste

#### Rischi principali
| Area | Evidenza | Impatto | Probabilita | Priorita |
|------|----------|---------|-------------|----------|

#### Opportunita
| Area | Problema attuale | Pattern suggerito | Beneficio atteso | Complessita |
|------|------------------|-------------------|------------------|-------------|

## Formato output 2: architecture-intervention-plan.md
Sezioni minime:
1. Obiettivi misurabili
2. Backlog interventi prioritizzato
3. Piano per fasi (Quick Wins, Mid-term, Strategic)
4. KPI tecnici da monitorare
5. Criteri di accettazione

### Tabella backlog interventi
| ID | Intervento | Pattern/Approccio | Area | Effort (S/M/L) | Impatto (1-5) | Rischio | Dipendenze | KPI collegato |
|----|------------|-------------------|------|----------------|---------------|--------|------------|---------------|

## Regole di qualita
- Non inventare dati o metriche: usa solo evidenze presenti nel codice
- Se manca un'informazione, dichiaralo esplicitamente
- Ogni proposta deve includere trade-off (pro/contro)
- Preferisci interventi incrementali e reversibili
- Indica sempre prima i Quick Wins ad alto impatto
- Evita refactor big-bang

## Pattern candidati (se pertinenti)
Valuta e proponi solo se supportati da evidenze:
- Endpoint composition patterns coerenti con Minimal API
- Query projection ottimizzata con Expression riusabili
- Caching selettivo (response/query) dove dimostrabile
- Resilience pattern per integrazioni esterne
- Consolidamento mapping/manual transformers
- Modularizzazione per domini funzionali
- Ottimizzazione EF Core (AsNoTracking, filtri, indici, split query quando utile)

## Deliverable addizionale (facoltativo ma consigliato)
- Crea `docs/adr/` con ADR sintetici per interventi ad alto impatto:
  - contesto
  - decisione
  - alternative scartate
  - conseguenze

## Risposta del prompt
- Rispondi solo con:
  1) file creati/aggiornati
  2) elenco dei top 5 interventi prioritari
  3) eventuali gap informativi bloccanti

## ✅ Checklist Post-Generazione
- [ ] Analisi basata su evidenze reali del repository
- [ ] Nessun pattern vietato proposto
- [ ] Findings e piano interventi generati in `docs/`
- [ ] Backlog con priorita, effort, impatto, KPI
- [ ] Quick Wins evidenziati chiaramente
- [ ] Trade-off dichiarati per i principali interventi

*Template v1.0 - .NET 10 - Token-optimized for AI agents* - Last Update 2026-03-02 00:00
````