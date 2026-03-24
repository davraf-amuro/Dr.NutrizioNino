# Backend Architecture Intervention Plan

## 1) Obiettivi

- Allineare `ValidatorMiddleware` alle route business reali.
- Uniformare `CancellationToken` su tutti i metodi repository I/O.
- Ridurre logging ad alto rischio (body, header, sensitive data).
- Introdurre suite di integration test minima sui path CRUD principali.
- Introdurre baseline metriche di latenza.

## 2) Backlog interventi

| ID | Intervento | Pattern/Approccio | Effort | Impatto (1-5) | Stato |
|----|------------|-------------------|--------|---------------|-------|
| B01 | Standardizzare route su `api/v{version:apiVersion}/...` | Route Groups versionati | M | 5 | ✅ Completato |
| B02 | Allineare `ValidatorMiddleware` alle route reali | Prefix coerente + ProblemDetails 401 | S | 5 | ✅ Completato |
| B03 | Correggere CORS per `PUT`, `DELETE` | Policy CORS allineata ai metodi esposti | S | 4 | ✅ Completato |
| B04 | Completare CRUD repository + validazioni | CRUD completo, duplicate check, FK protection | M | 5 | ✅ Completato |
| B05 | Uniformare async/EF Core | `SaveChangesAsync`, `CancellationToken` uniforme | M | 4 | ⚠️ Parziale |
| B06 | Ridurre logging body/header | Structured logging con redaction | S | 4 | ⚠️ Parziale |
| B07 | Rafforzare OpenAPI metadata | `Produces`, `WithName`, `WithSummary` uniformi | M | 3 | ✅ Completato |
| B08 | Integration test endpoint core | Happy-path + not-found + conflict | M | 4 | ⚠️ Da fare |
| B09 | Baseline metriche backend | Logging latenza endpoint + query count | M | 3 | ⚠️ Da fare |

## 3) Piano per fasi aggiornato

### Quick Wins (prossimo sprint)

- **B05**: completare `CancellationToken` nei metodi repository mancanti
- **B06**: disabilitare `EnableSensitiveDataLogging` in produzione (header già filtrati, manca solo questo)

### Mid-term (1-2 sprint)

- **B08**: integration test per endpoint Foods, Nutrients, UnitsOfMeasures, Brands
  - happy-path GET/POST/PUT/DELETE
  - not-found (404)
  - conflict (409) su duplicate e FK violation

### Strategic

- **B09**: baseline metriche P95 latenza endpoint principali
- Hardening continuo osservabilità e sicurezza

## 4) KPI tecnici

| KPI | Target |
|-----|--------|
| % endpoint con route versionata | 100% ✅ |
| % endpoint con metadata OpenAPI completa | 100% ✅ |
| Errori runtime da metodi non implementati | 0 ✅ |
| Middleware allineato alle route reali | ✅ |
| % metodi repository con `CancellationToken` | < 100% — da completare |
| Copertura integration test endpoint core | Da misurare |
| `EnableSensitiveDataLogging` disabilitato in prod | ⚠️ Da fare |

## 5) Criteri di accettazione aperti

- `ValidatorMiddleware` protegge effettivamente le route business `/api/v{version}/...`. ✅
- Tutti i metodi repository I/O accettano `CancellationToken`.
- Nessun log in produzione contiene body request o header di autenticazione raw.
- Suite integration test copre almeno i path CRUD principali con scenari happy-path, not-found e conflict.

---
*Ultima revisione: 2026-03-24 | Focus: Backend API*
