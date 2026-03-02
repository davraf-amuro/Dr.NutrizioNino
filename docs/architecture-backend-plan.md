# Backend Architecture Intervention Plan

## 1) Obiettivi misurabili
- Applicare versioning URL a tutti gli endpoint backend.
- Eliminare i bypass di validazione middleware sui path business.
- Portare a completamento i metodi repository attualmente incompleti/placeholder.
- Uniformare data-access read/write a pattern async coerenti.
- Introdurre un minimo set di test di regressione su endpoint core.

## 2) Backlog interventi prioritizzato
| ID | Intervento | Pattern/Approccio | Area API | Effort (S/M/L) | Impatto (1-5) | Rischio | Dipendenze | KPI |
|----|------------|-------------------|----------|----------------|---------------|--------|------------|-----|
| B01 | Standardizzare tutte le route su `api/v{version:apiVersion}/...` | Route Groups versionati + `WithApiVersionSet` + `MapToApiVersion` | Endpoints | M | 5 | Medio | Aggiornamento endpoint + file `.http` | % endpoint versionati |
| B02 | Allineare `ValidatorMiddleware` alle route reali | Security middleware coerente col prefix API + risposte 401 strutturate | Middleware | S | 5 | Basso | B01 | % richieste protette su route business |
| B03 | Correggere CORS per metodi realmente esposti (`PUT`,`DELETE`) | Policy CORS allineata ai metodi endpoint effettivi | Program/Pipeline | S | 4 | Basso | Nessuna | Errori CORS client |
| B04 | Completare i metodi repository incompleti | Implementazione CRUD completa, rimozione placeholder/throw | Infrastructure | M | 5 | Medio | Revisione Service usage | Errori runtime da metodi non implementati |
| B05 | Uniformare async/EF Core I/O | `SaveChangesAsync`, `ToListAsync`, `CancellationToken` ultimo parametro | Infrastructure/Services | M | 4 | Medio | B04 | % metodi I/O con CT + async reale |
| B06 | Ridurre logging ad alto costo/rischio | Structured logging con mascheramento campi sensibili e riduzione body logging | Middleware/Observability | S | 4 | Basso | Definizione policy logging | Volume log/request + eventi con PII |
| B07 | Rafforzare OpenAPI metadata endpoint | `Produces`, `WithName`, `WithSummary`, `WithDescription` uniformi | Endpoints | M | 3 | Basso | B01 | % endpoint con metadata completa |
| B08 | Aggiungere integration test minimi su endpoint core | Test happy-path + not-found + validation | Testing | M | 4 | Medio | B01/B04 | Copertura endpoint core |
| B09 | Introdurre baseline metriche backend | Logging/misure per latenza endpoint e query count | Operability | M | 3 | Medio | B06 | P95 endpoint principali, query/request |

## 3) Piano per fasi

### Quick Wins (1 sprint)
- B03: allineamento CORS ai metodi endpoint esposti.
- B02: fix controllo path in `ValidatorMiddleware`.
- B06: riduzione logging body/header raw e redaction minima.
- B07: metadata OpenAPI minima uniforme per endpoint privi.

### Mid-term (1-2 sprint)
- B01: migrazione completa route/versioning endpoint.
- B04: completamento metodi repository oggi incompleti.
- B05: uniformita async I/O + no-tracking/materializzazione coerente.
- B08: suite integrazione minima per endpoint core.

### Strategic (2+ sprint)
- B09: baseline metriche performance e quality gates.
- Revisione periodica dei contratti API e compatibilita versione.
- Hardening continuo su sicurezza e osservabilita.

## 4) KPI tecnici da monitorare
- `% endpoint con route versionata`.
- `% endpoint con metadata OpenAPI completa`.
- `Numero eccezioni runtime da metodi non implementati`.
- `P95 latenza endpoint principali` (dopo baseline).
- `Errori CORS lato client`.
- `% metodi repository I/O con async reale + CancellationToken`.

## 5) Criteri di accettazione
- Tutti i group endpoint backend rispettano formato `api/v{version:apiVersion}/...`.
- `ValidatorMiddleware` protegge effettivamente le route business previste.
- Nessun metodo repository critico rimane placeholder o `NotImplemented`.
- Nessun metodo async I/O usa `SaveChanges` sync o pattern placeholder non necessari.
- Esiste una suite integration test minima che copre endpoint CRUD principali.

## Trade-off principali
- Migrazione route/versioning richiede update consumer e file `.http`, ma riduce debito tecnico futuro.
- Ridurre logging raw migliora sicurezza/performance, ma richiede definire a monte i campi diagnostici minimi.
- Uniformare async/CT aumenta disciplina del codice, con un piccolo costo iniziale di refactor.

---
*Piano generato il: 2026-03-02 | Focus: Backend API | LLM: GitHub Copilot*