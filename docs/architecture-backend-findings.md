# Backend Architecture Findings

## 1) Executive Summary
Il backend adotta Minimal API con separazione `Endpoints -> Services -> Repository -> DbContext`, ma risulta parzialmente allineato ai vincoli architetturali dichiarati nel progetto.

Le criticita principali sono: versioning configurato ma non applicato alle route, protezione middleware agganciata a `/api/` mentre gli endpoint non usano quel prefisso, metodi repository incompleti o con implementazioni placeholder, e pratiche async/EF Core non uniformi.

La base e recuperabile con interventi incrementali: standardizzare route/versioning, completare i metodi TODO, uniformare query/read model no-tracking, e migliorare logging/sicurezza per ridurre rischio operativo e overhead.

## 2) Mappa architettura corrente
- Entry point: `src/Dr.NutrizioNino.Api/Program.cs`
- Endpoint mapping: `src/Dr.NutrizioNino.Api/Endopints/*Endpoints.cs`
- Application service: `src/Dr.NutrizioNino.Api/Services/DrService*.cs`
- Data access: `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository*.cs`
- EF Core context: `src/Dr.NutrizioNino.Api/Infrastructure/DrNutrizioNinoContext.cs`
- Middleware custom: `src/Dr.NutrizioNino.Api/Middleware/HttpContextLogger.cs`, `src/Dr.NutrizioNino.Api/Middleware/ValidatorMiddleware.cs`
- OpenAPI transformers: `src/Dr.NutrizioNino.Api/Transformers/*`
- Test backend: `src/Testing/Dr.NutrizioNino.Api.Test/FoodsTest.cs`

## 3) Rischi principali
| Area | Evidenza | Impatto | Probabilita | Priorita |
|------|----------|---------|-------------|----------|
| API versioning | In `Program.cs` e presente `AddApiVersioning` + `ApiVersionSet`, ma i group endpoint usano route tipo `foods`, `brands` senza `api/v{version:apiVersion}` e senza `WithApiVersionSet/MapToApiVersion` | Contratti API non versionati, incompatibilita evolutive | Alta | Alta |
| Sicurezza request | `ValidatorMiddleware` valida solo path contenenti `/api/`, ma gli endpoint mappati non hanno prefisso `/api/` | Bypass della validazione token/origin per endpoint business | Alta | Alta |
| CORS | Policy CORS consente solo `GET`,`POST` ma esistono endpoint `PUT`,`DELETE` | Errori browser e blocchi integrazione frontend | Alta | Alta |
| Integrita funzionale | Repository con metodi non implementati (`throw`) e placeholder (es. `GetUnitOfMeasureAsync` ritorna oggetto vuoto) | Errori runtime e risultati incoerenti | Alta | Alta |
| Async/EF consistency | Metodi `async` senza `await`, ritorno diretto di `DbSet`, uso misto `SaveChanges` sync in metodi async | Overhead inutile, comportamento non uniforme, difficolta tuning | Media | Alta |
| Logging/sensibilita dati | `HttpContextLogger` logga header completi + body; DbContext abilita `EnableSensitiveDataLogging` | Rischi privacy/security e costo I/O log elevato | Media | Media |
| Test coverage | Test API presenti ma sostanzialmente vuoti/commentati | Regressioni non intercettate | Alta | Alta |

## 4) Opportunita di miglioramento
| Area | Problema attuale | Pattern suggerito | Beneficio atteso | Complessita |
|------|------------------|-------------------|------------------|-------------|
| Endpoint contract | Route non standardizzate e non versionate | Route group standard `api/v{version:apiVersion}/{group}` + `WithApiVersionSet` + `MapToApiVersion` | Contratti stabili, evoluzione versioni controllata | M |
| Security boundary | Check `/api/` non allineato alle route reali | Allineare route prefix e validazione; usare ProblemDetails/typed responses per 401 | Riduzione rischio bypass + diagnosi migliore | S |
| Data access | Metodi incompleti/placeholder e async incoerente | Completare CRUD + uniformare async (`SaveChangesAsync`, `ToListAsync`, `CancellationToken`) | Affidabilita runtime e migliore latenza sotto carico | M |
| Read performance | Query read non sempre materializzate/ottimizzate | Pattern query read-only no-tracking + projection esplicite riusabili | Minori allocazioni e query piu predicibili | M |
| Logging | Logging raw request/body generalizzato | Structured logging con redaction + sampling su payload | Riduzione rumore e rischio leak dati | S |
| Governance qualità | Test non coprono i path critici | Integration tests per endpoint principali + smoke suite CI | Maggiore confidenza nei refactor | M |

## 5) Anti-pattern trovati (evidenze reali)
- Versioning configurato ma non applicato ai route group endpoint.
- Middleware di sicurezza condizionato da un prefisso route non usato dagli endpoint effettivi.
- Implementazioni parziali in repository con eccezioni `NotImplemented` e ritorni placeholder.
- Metodi dichiarati `async` che non effettuano `await` reale e ritorni `Task.FromResult` superflui.
- Logging di request body/header completo senza chiara strategia di mascheramento.

## Note su metriche
Non emergono nel repository misure baseline (es. P95, query/request, throughput) gia versionate; e necessario introdurre instrumentation minima prima di stimare delta quantitativi.

---
*Documento generato il: 2026-03-02 | Focus: Backend API | LLM: GitHub Copilot*
