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
| B02 | Sicurezza request | `ValidatorMiddleware` valida path con `/api/` ma gli endpoint non usano quel prefisso — bypass token/origin possibile | Alto | Alta | ⚠️ Aperto |
| B03 | CORS | Policy include `PUT`, `DELETE` | Alto | Alta | ✅ Risolto |
| B04 | Integrità funzionale | CRUD completo per Foods, Brands, Nutrients, UnitsOfMeasures. Validazione duplicati e FK protection su Nutrients e UdM | Alto | Alta | ✅ Risolto |
| B05 | Async/EF consistency | Alcuni metodi mancano di `CancellationToken`. `SaveChangesAsync` e `ToListAsync` usati, ma non uniformi | Media | Alta | ⚠️ Parziale |
| B06 | Logging/sensibilità dati | `HttpContextLogger` logga header completi + body. `EnableSensitiveDataLogging` attivo | Media | Media | ⚠️ Aperto |
| B08 | Test coverage | Test di integrazione presenti ma con copertura minima | Alta | Alta | ⚠️ Aperto |
| B09 | Metriche baseline | Nessuna misura P95/query-per-request versionata | Media | Media | ⚠️ Aperto |

## 4) Opportunità di miglioramento residue

| Area | Problema attuale | Pattern suggerito | Beneficio atteso | Complessità |
|------|------------------|-------------------|------------------|-------------|
| Security boundary | `ValidatorMiddleware` controlla `/api/` ma gli endpoint usano quel prefisso dopo il versioning — verificare allineamento | Allineare prefix middleware alla route reale | Protezione token/origin efficace su tutti gli endpoint business | S |
| Async/CT | Metodi repository senza `CancellationToken` | Aggiungere `CancellationToken` come ultimo parametro ai metodi I/O | Cancellazione corretta e maggiore disciplina | S |
| Logging | Body e header loggati senza redaction | Structured logging con mascheramento campi sensibili | Riduzione rischio leak dati e volume log | S |
| Test | Suite di integrazione ridotta | Integration test happy-path + not-found + conflict per endpoint CRUD | Confidenza nei refactor | M |

## 5) Anti-pattern risolti

| Anti-pattern | Risolto con |
|--------------|-------------|
| Route non versionate | Tutti i group endpoint usano `api/v{version:apiVersion}/...` |
| CRUD repository incompleto | Tutti i metodi CRUD implementati per tutte le entità |
| Metadata OpenAPI mancante | Tutti gli endpoint hanno `Produces`, `WithName`, `WithSummary`, `WithDescription` |
| Nessuna validazione duplicati | Check su nome/abbreviazione su create/update per Nutrients e UnitsOfMeasures |
| Delete senza FK protection | Block delete con 409 Conflict se record in uso (Nutrients → Foods_Nutrients; UdM → Foods, Foods_Nutrients, Nutrients) |

## 6) Anti-pattern ancora presenti

- `ValidatorMiddleware` condizionato da prefisso route potenzialmente non allineato.
- Metodi `async` senza `CancellationToken` nei repository.
- `HttpContextLogger` logga request body/header senza redaction.
- Test di integrazione con copertura insufficiente.

---
*Ultima revisione: 2026-03-18 | Focus: Backend API*
