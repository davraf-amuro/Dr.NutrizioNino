# Backend Architecture — Piano interventi

## Obiettivi

- Uniformare `CancellationToken` su tutti i metodi repository I/O.
- Rimuovere `EnableSensitiveDataLogging` dalla configurazione di produzione.
- Introdurre suite di integration test sui path CRUD principali.
- Baseline metriche di latenza endpoint.

---

## Backlog

| ID | Intervento | Effort | Stato |
|----|------------|--------|-------|
| B01 | Versioning URL `api/v{version}/...` su tutti gli endpoint | M | ✅ Completato |
| B02 | `ValidatorMiddleware` allineato alle route reali | S | ✅ Completato |
| B03 | CORS per `PUT`, `DELETE` | S | ✅ Completato |
| B04 | CRUD completo, validazioni duplicati, FK protection | M | ✅ Completato |
| B05 | `CancellationToken` uniforme nei repository | M | ⚠️ Parziale |
| B06 | Logging: disabilitare `EnableSensitiveDataLogging` in prod | S | ⚠️ Parziale |
| B07 | OpenAPI metadata uniformi (`Produces`, `WithName`, `WithSummary`) | M | ✅ Completato |
| B08 | Integration test — happy-path + 404 + 409 per endpoint CRUD | M | ⚠️ Da fare |
| B09 | Baseline metriche latenza P95 | M | ⚠️ Da fare |

---

## Prossime priorità

### Quick Wins

- **B05**: aggiungere `CancellationToken` ai metodi repository che ne sono privi.
- **B06**: rimuovere o condizionare `EnableSensitiveDataLogging` all'ambiente di sviluppo.

### Mid-term

- **B08**: integration test per endpoint Foods, Nutrients, Categories, Supermarkets:
  - happy-path GET / POST / PUT / DELETE
  - not-found (404)
  - conflict (409) su duplicati e FK in uso

### Strategic

- **B09**: misurare P95 latenza e query-count per i path principali.

---

## KPI

| KPI | Target | Stato |
|-----|--------|-------|
| Endpoint con route versionata | 100% | ✅ |
| Endpoint con metadata OpenAPI completa | 100% | ✅ |
| Errori runtime da metodi non implementati | 0 | ✅ |
| Middleware allineato alle route reali | ✅ | ✅ |
| Metodi repository con `CancellationToken` | 100% | ⚠️ Parziale |
| `EnableSensitiveDataLogging` disabilitato in prod | ✅ | ⚠️ Da fare |
| Copertura integration test endpoint core | ≥ happy-path + 404 + 409 | ⚠️ Da fare |

---

*Ultima revisione: 2026-04-03 — modello `claude-sonnet-4-6`*
