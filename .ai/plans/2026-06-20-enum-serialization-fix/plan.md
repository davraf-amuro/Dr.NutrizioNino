# Piano: enum-serialization-fix

**Data:** 2026-06-20  
**Stato:** COMPLETATO

## Obiettivo

Correggere il mismatch di serializzazione tra l'enum C# `ExtractionStatus` (serializzato come intero) e il tipo TypeScript `ExtractionStatus` (stringa). Causa: tre comparazioni rotte nel FE che rendono la feature AI vision silenziosa.

## Scope

**File da modificare:**
- `src/Dr.NutrizioNino.Api/Program.cs` — aggiungere `JsonStringEnumConverter` a `ConfigureHttpJsonOptions`

**Perimetro negativo:**
- Nessuna modifica al FE (Vue, TypeScript)
- Nessuna modifica ai DTO C# o all'enum `ExtractionStatus`
- Nessuna migrazione DB

## Fasi

- [x] 1. Aggiungere `JsonStringEnumConverter` in `Program.cs` tramite `ConfigureHttpJsonOptions`
- [x] 2. Lint gate: `dotnet format src/Dr.NutrizioNino.Api/Dr.NutrizioNino.Api.csproj --verify-no-changes` — fix pre-esistente in `CacheCleanupService.cs` (IDE1006: `_interval`)

## Criteri di verifica

- `Program.cs` contiene `ConfigureHttpJsonOptions` con `JsonStringEnumConverter`
- Lint esce con code 0
- Grep FE su `status ===` non mostra nuovi confronti su enum numerici

## Note

Grep preventivo eseguito: tutti gli altri `status ===` nel FE sono su `ApiError.status` (HTTP codes 401/409) — nessun conflitto con la modifica globale.
