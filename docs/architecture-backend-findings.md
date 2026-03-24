# Backend Architecture Findings

## 1) Executive Summary

Il backend adotta Minimal API con separazione `Endpoints → Services → Repository → DbContext`.
Dall'analisi iniziale (2026-03-02) sono stati risolti i problemi principali di versioning, metodi repository incompleti e metadata OpenAPI.

Rimangono aperti: allineamento del middleware di sicurezza alle route reali, uniformità async/EF Core con `CancellationToken`, logging strutturato con redaction, test di integrazione e baseline metriche.

## 2) Mappa architettura corrente

| Livello | File/Cartella |
|---------|---------------|
| Entry point | `src/Dr.NutrizioNino.Api/Program.cs` |
| Route handlers | `src/Dr.NutrizioNino.Api/Endopints/*Endpoints.cs` |
| Business logic | `src/Dr.NutrizioNino.Api/Services/DrService*.cs` |
| Data access | `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository*.cs` |
| EF Core context | `src/Dr.NutrizioNino.Api/Infrastructure/DrNutrizioNinoContext.cs` |
| Middleware | `Middleware/HttpContextLogger.cs`, `Middleware/ValidatorMiddleware.cs` |
| OpenAPI transformers | `src/Dr.NutrizioNino.Api/Transformers/` |
| Test backend | `src/Testing/Dr.NutrizioNino.Api.Test/` |

## 3) Stato dei rischi

| ID | Area | Evidenza | Impatto | Priorità | Stato |
|----|------|----------|---------|----------|-------|
| B01 | API versioning | Route group con `api/v{version:apiVersion}/...`, `WithApiVersionSet`, `MapToApiVersion` su tutti gli endpoint | Alto | Alta | ✅ Risolto |
| B02 | Sicurezza request | `ValidatorMiddleware` controlla `StartsWithSegments("/api")` — gli endpoint usano `/api/v{version}/...`, il prefisso è allineato | Alto | Alta | ✅ Risolto |
| B03 | CORS | Policy include `PUT`, `DELETE` | Alto | Alta | ✅ Risolto |
| B04 | Integrità funzionale | CRUD completo per Foods, Brands, Nutrients, UnitsOfMeasures. Validazione duplicati e FK protection su Nutrients e UdM | Alto | Alta | ✅ Risolto |
| B05 | Async/EF consistency | Alcuni metodi mancano di `CancellationToken`. `SaveChangesAsync` e `ToListAsync` usati, ma non uniformi | Media | Alta | ⚠️ Parziale |
| B06 | Logging/sensibilità dati | `HttpContextLogger` filtra header sensibili via `SensitiveHeaders` HashSet. `EnableSensitiveDataLogging` ancora attivo in `Program.cs` | Media | Media | ⚠️ Parziale |
| B08 | Test coverage | Test di integrazione presenti ma con copertura minima | Alta | Alta | ⚠️ Aperto |
| B09 | Metriche baseline | Nessuna misura P95/query-per-request versionata | Media | Media | ⚠️ Aperto |

## 4) Bug corretti

| ID | Area | Fix applicato |
|----|------|---------------|
| B10 | Precisione colonna DB | `Foods_Nutrients.Quantity`: `numeric(4,2)` → `numeric(6,2)` — valore `155,00` causava `DbUpdateException` |

## 5) Opportunità di miglioramento residue

| Area | Problema attuale | Pattern suggerito | Beneficio atteso | Complessità |
|------|------------------|-------------------|------------------|-------------|
| Security boundary | `ValidatorMiddleware` controlla `/api/` ma gli endpoint usano quel prefisso dopo il versioning — verificare allineamento | Allineare prefix middleware alla route reale | Protezione token/origin efficace su tutti gli endpoint business | S |
| Async/CT | Metodi repository senza `CancellationToken` | Aggiungere `CancellationToken` come ultimo parametro ai metodi I/O | Cancellazione corretta e maggiore disciplina | S |
| Logging | Body e header loggati senza redaction | Structured logging con mascheramento campi sensibili | Riduzione rischio leak dati e volume log | S |
| Test | Suite di integrazione ridotta | Integration test happy-path + not-found + conflict per endpoint CRUD | Confidenza nei refactor | M |

## 6) Anti-pattern risolti

| Anti-pattern | Risolto con |
|--------------|-------------|
| Route non versionate | Tutti i group endpoint usano `api/v{version:apiVersion}/...` |
| CRUD repository incompleto | Tutti i metodi CRUD implementati per tutte le entità |
| Metadata OpenAPI mancante | Tutti gli endpoint hanno `Produces`, `WithName`, `WithSummary`, `WithDescription` |
| Nessuna validazione duplicati | Check su nome/abbreviazione su create/update per Nutrients e UnitsOfMeasures |
| Delete senza FK protection | Block delete con 409 Conflict se record in uso (Nutrients → Foods_Nutrients; UdM → Foods, Foods_Nutrients, Nutrients) |
| Middleware non allineato alle route | `ValidatorMiddleware` ora controlla correttamente il prefisso `/api` |
| Header autenticazione loggati in chiaro | `HttpContextLogger` filtra `authorization`, `cookie`, `set-cookie`, `internal-authorization` |

## 7) Anti-pattern ancora presenti

- Metodi `async` senza `CancellationToken` nei repository.
- `EnableSensitiveDataLogging` attivo in `Program.cs` (EF Core logga valori parametri SQL in dev).
- Test di integrazione con copertura insufficiente.

---
*Ultima revisione: 2026-03-24 | Focus: Backend API*
