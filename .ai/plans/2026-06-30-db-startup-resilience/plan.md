# Piano — Resilienza DB all'avvio API + stato/retry

Stato: COMPLETATO
Data: 2026-06-30
Tipo progetto: Minimal API (.NET 10)

## Obiettivo

Se il database non è raggiungibile all'avvio delle API:
1. **No crash** — l'app deve partire comunque (oggi `EnsureRolesExistAsync` lancia `SqlException` → `throw` in Program.cs → processo termina).
2. **Errore loggato** in modo strutturato.
3. **Errore esposto al FE** — gli endpoint che usano il DB già rispondono `503 ProblemDetails` via `DatabaseExceptionHandler`. Aggiungere un endpoint di **stato** consultabile dal FE.
4. **Retry** — endpoint che ritenta la connessione + seed quando l'utente ha risolto.

## Scope

### File da modificare
- `src/Dr.NutrizioNino.Api/Program.cs`
  - Sostituire il blocco seed sincrono (righe ~229-234) con chiamata resiliente in try/catch che logga e **non** rilancia.
  - Registrare `DatabaseStartupService` come singleton.
  - Mappare `MapHealthEndpoints(versionSet)`.

### File da creare
- `src/Dr.NutrizioNino.Api/Services/DatabaseStartupService.cs`
  - Singleton. Mantiene stato (`IsDatabaseReady`, `LastError`, `LastCheckedUtc`).
  - Usa `IServiceScopeFactory` (perché `AdminUserService`/`RoleManager` sono scoped) + `IDbContextFactory<DrNutrizioNinoContext>`.
  - `TryInitializeAsync(ct)`: `CanConnectAsync` → se ok esegue seed ruoli → aggiorna stato. Cattura eccezioni, logga, ritorna bool. Mai rilancia.
- `src/Dr.NutrizioNino.Api/Endpoints/HealthMapping.cs`
  - `GET  /api/v1/status` — stato corrente (live `CanConnect` + ultimo errore). 200 sempre, body con `databaseReady`.
  - `POST /api/v1/status/retry-database` — riesegue `TryInitializeAsync`. 200 se ora pronto, 503 ProblemDetails se ancora down.
  - Metadata OpenAPI completi (WithSummary/Description/Tags/Name/Produces) — Scalar.
- `src/Dr.NutrizioNino.Api/Endpoints/Health.http` — chiamate di test per i due endpoint.

### Perimetro negativo (NON toccare)
- Endpoint esistenti e loro handler.
- `DatabaseExceptionHandler` (già gestisce il 503 a runtime).
- Schema/migration DB.
- `AdminUserService` logica esistente (riuso `EnsureRolesExistAsync`, già `internal`).

## Fasi

- [x] F1 — Creare `Services/DatabaseStartupService.cs` (singleton, scope factory, stato + TryInitializeAsync)
- [x] F2 — Creare `Endpoints/HealthMapping.cs` (GET status + POST retry-database, metadata OpenAPI)
- [x] F3 — Modificare `Program.cs` (DI singleton, seed resiliente no-throw, MapHealthEndpoints)
- [x] F4 — Creare `Health.http` (root progetto, coerente con gli altri .http)
- [x] F5 — Build `dotnet build` clean (0 errori)
- [x] F6 — Verifica finale criteri

## Criteri di verifica

- [ ] Avvio con DB down: app parte, nessuna eccezione non gestita, log `Warning/Error` strutturato presente.
- [ ] `GET /api/v1/status` con DB down → `databaseReady=false`; con DB up → `true`.
- [ ] `POST /api/v1/status/retry-database` con DB tornato up → 200 e ruoli seedati; con DB ancora down → 503 ProblemDetails.
- [ ] Endpoint DB-dipendenti a runtime con DB down → 503 (comportamento `DatabaseExceptionHandler` invariato).
- [ ] `dotnet build` senza errori; commenti `///` su metodi pubblici (regola 13).
