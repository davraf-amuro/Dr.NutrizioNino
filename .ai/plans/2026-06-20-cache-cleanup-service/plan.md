# Piano: CacheCleanupService — TTL 24h+1m

**Stato:** COMPLETATO
**Data:** 2026-06-20
**Slug:** cache-cleanup-service

## Obiettivo
Background job che all'avvio e ogni 24h elimina le righe di `NutrientExtractionCache` con `CreatedAt` più vecchio di 24 ore e 1 minuto.

## Scope

**File da modificare:**
- `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository.NutrientExtractionCache.cs` — aggiunge `DeleteExpiredCacheAsync`
- `src/Dr.NutrizioNino.Api/Program.cs` — registra `AddHostedService<CacheCleanupService>`

**File da creare:**
- `src/Dr.NutrizioNino.Api/Services/CacheCleanupService.cs` — BackgroundService con PeriodicTimer

**Perimetro negativo:** nessun tocco a entità, migration, endpoint, VisionExtractionService, frontend.

## Fasi

- [x] 1. Aggiungere `DeleteExpiredCacheAsync(DateTime cutoff, CancellationToken ct)` in `DrRepository.NutrientExtractionCache.cs` via `ExecuteDeleteAsync`
- [x] 2. Creare `Services/CacheCleanupService.cs` — `BackgroundService` con `IServiceScopeFactory`, `PeriodicTimer(24h)`, run immediato all'avvio
- [x] 3. Registrare in `Program.cs`: `builder.Services.AddHostedService<CacheCleanupService>()`
- [x] 4. Verifica build: compilazione clean — file lock da debugger attivo (DLL occupata), nessun errore CS

## Criteri di verifica
- [ ] Build verde senza warning
- [ ] Log di startup mostra "Cache cleanup: X record eliminati"
- [ ] Nessuna dipendenza circolare (IServiceScopeFactory usata correttamente — DbContext è Scoped)

## Decisioni tecniche
- TTL: `TimeSpan.FromHours(24).Add(TimeSpan.FromMinutes(1))`
- Intervallo poll: `PeriodicTimer(TimeSpan.FromHours(24))`
- Scope: `IServiceScopeFactory` per risolvere `DrRepository` (Scoped) da un Singleton/BackgroundService
- `ExecuteDeleteAsync` — delete diretto sul DB, nessun load in memoria
