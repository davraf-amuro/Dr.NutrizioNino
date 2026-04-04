# Onboarding — Dr.NutrizioNino

## Il progetto in tre righe

Applicazione web per la gestione di un diario alimentare. Permette di censire alimenti con i loro nutrienti, costruire piatti come distinte base di ingredienti, e gestire le anagrafiche di supporto (marche, unità di misura, supermercati, categorie). Autenticazione multi-utente con ruoli `User` e `Admin`.

---

## Stack e scelte tecniche

### Backend

| Tecnologia | Versione | Note |
|---|---|---|
| .NET / C# | 10 / 14 | Target framework `net10.0` |
| ASP.NET Core Minimal API | 10.0.3 | No MVC controllers — scelta esplicita di progetto |
| Entity Framework Core | 10.0.3 | Code-first, mapping manuale via `IEntityTypeConfiguration<T>` |
| SQL Server | — | Connection string in `appsettings.local.json` |
| ASP.NET Core Identity | 10.0.3 | `ApplicationUser : IdentityUser<Guid>`, ruoli via `IdentityRole<Guid>` |
| JWT Bearer | 10.0.3 | HMAC-SHA256, 8 ore, localStorage nel frontend |
| Asp.Versioning | 8.1.1 | `UrlSegmentApiVersionReader` — versione nell'URL, non nell'header |
| Scalar | 2.12.50 | Sostituisce Swagger UI — documentazione su `/scalar/v1` |
| Serilog | 10.0.0 | Console + file rotante, configurazione da `appsettings.json` |
| TinyHelpers.AspNetCore | 4.1.18 | `AddDefaultProblemDetails`, `ProducesDefaultProblem` |

### Frontend

| Tecnologia | Note |
|---|---|
| Vue 3 + TypeScript | Composition API, `<script setup>` |
| Vite 8 | Dev server su `localhost:5173` |
| Naive UI | Design system — tutti i componenti UI vengono da qui |
| Axios | HTTP client con interceptor per header `Authorization: Bearer` |
| Vue Router 4 | Navigation guard con controllo JWT locale prima di chiamare `/me` |

---

## Come avviare il progetto in locale

### Prerequisiti

- .NET 10 SDK
- Node.js 20+
- SQL Server raggiungibile con database `DrNutrizioNino` già creato
- Submoduli Git inizializzati

### 1. Clona con submoduli

```bash
git clone https://github.com/davraf-amuro/Dr.NutrizioNino.git
cd Dr.NutrizioNino
git submodule update --init --recursive
```

### 2. Configura il backend

Crea `src/Dr.NutrizioNino.Api/appsettings.local.json` (non viene committato):

```json
{
  "ConnectionStrings": {
    "DrNutrizioNinoSql": "Data Source=<server>;Initial Catalog=DrNutrizioNino;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
  },
  "Jwt": {
    "Secret": "<chiave-min-32-caratteri>"
  },
  "AllowedOrigins": [ "http://localhost:5173" ]
}
```

### 3. Avvia il backend

```bash
cd src/Dr.NutrizioNino.Api
dotnet run
```

API: `http://localhost:5083` — Scalar: `http://localhost:5083/scalar/v1`

### 4. Avvia il frontend

```bash
cd src/Dr.NutrizioNino.WebVue
npm install
npm run dev
```

Frontend: `http://localhost:5173`

### Alternativa VSCode

Usa la configurazione **Full Stack (API + Vue)** nel pannello *Run & Debug* (F5) per avviare entrambi insieme.

### Schema DB

Lo schema è gestito tramite script SQL in `schema-migrations/`. Non ci sono EF Core migrations — il DB va allineato manualmente applicando i file `.sql` in ordine cronologico.

---

## Struttura del codice

### Backend — `src/Dr.NutrizioNino.Api/`

```
Endopints/          Route handlers — ogni dominio è un extension method Map*Endpoints()
Services/           Business logic — BrandService, FoodService, DishService, ecc.
Infrastructure/
  DrNutrizioNinoContext*.cs   DbContext (partial, un file per dominio aggregato)
  DrRepository*.cs            Repository (partial, un file per dominio)
  ModelsFactory.cs            Costruzione entità da DTO
  Models/                     Entità EF Core
  Models/Configurations/      IEntityTypeConfiguration<T> per ogni entità
Middleware/
  HttpContextLogger.cs        Log richieste/risposte (filtra header sensibili)
  ValidatorMiddleware.cs      Validazione request su path /api
Transformers/                 Transformer OpenAPI (AddDocumentInformations, AddHeaders, ecc.)
Middleware/DatabaseExceptionHandler.cs   Intercetta SqlException → 503
```

**Dove tocchi più spesso:**
- Nuovo endpoint → `Endopints/` + registrazione in `Program.cs`
- Nuova logica → `Services/`
- Nuovo accesso DB → `DrRepository.<dominio>.cs`
- Nuova entità → `Models/` + `Configurations/` + `DrNutrizioNinoContext*.cs`

### Frontend — `src/Dr.NutrizioNino.WebVue/src/`

```
modules/<dominio>/api/           Chiamate HTTP (axios) per feature
modules/<dominio>/composables/   State management reattivo (useXxx)
components/<Dominio>/            Componenti UI per dominio
views/                           Pagine — una per rotta principale
core/
  http/apiClient.ts              Istanza Axios con interceptor auth + errori
  http/ApiError.ts               Classe errore tipizzata (status, title, detail)
  http/tokenStorage.ts           Getter/setter localStorage per JWT
  composables/useAsyncState.ts   Pattern async/loading/error condiviso
  composables/useTableSearch.ts  Ricerca e filtro tabelle
  composables/useDishCalculator.ts  Calcolo nutrienti piatto da ingredienti
  utils/sortNutrients.ts         Ordinamento nutrienti (positionOrder → alfa)
modules/auth/composables/useAuth.ts   Stato autenticazione singleton
router/index.ts                  Route + navigation guard
```

### Modelli condivisi — `Dr.NutrizioNino.Models/Dto/`

DTO usati sia dall'API che (come riferimento) dal frontend. Progetto .NET separato referenziato dall'API.

---

## Convenzioni obbligatorie

### Backend

| Regola | Dettaglio |
|---|---|
| Solo Minimal API | Niente MVC Controllers, niente AutoMapper, niente MediatR |
| Endpoint in extension method | `public static IEndpointRouteBuilder Map*Endpoints(this IEndpointRouteBuilder, ApiVersionSet)` |
| Route format | `api/v{version:apiVersion}/{risorsa}` — sempre |
| Metadata OpenAPI | `Produces` + `WithName` + `WithSummary` + `WithDescription` obbligatori |
| Errori | `TypedResults.Problem(new ProblemDetails { ... })` — mai `throw` nei handler |
| FK pre-check su DELETE | Prima di cancellare un record, verifica che non sia referenziato; ritorna 409 se in uso |
| Ordine parametri handler | route → query → body → servizi DI → `CancellationToken` ultimo |
| Logging | Serilog con placeholder strutturati — `Log.Information("Testo {Param}", val)` |

### Frontend

| Regola | Dettaglio |
|---|---|
| Async state | Sempre tramite `useAsyncState` — non gestire loading/error a mano |
| Cache | Ogni composable mantiene una cache in-memory 60s; `load(force: true)` per invalidare |
| Errori HTTP | Intercettati da `apiClient`; usare `ApiError` per accedere a `status`, `title`, `detail` |
| Componenti UI | Solo Naive UI — non mescolare librerie |

### Ciclo di sviluppo (AI-agent e umano)

Prima di modificare codice: dichiara file + motivo + scope negativo. Una modifica per turno. Rileggi dopo ogni modifica per verificare. Vedi `.github/instructions/dev-cycle.instructions.md`.

---

## Flusso di lavoro

| Aspetto | Dettaglio |
|---|---|
| Branch principale | `master` — stabile |
| Branch di sviluppo | `dev` — qui si lavora normalmente |
| PR | `dev` → `master` via pull request |
| Commit | Formato `type(scope): descrizione` — es. `feat(foods): aggiungi colonna categoria` |
| CI/CD | Non configurato al momento |
| Schema DB | File `.sql` in `schema-migrations/` con nome `YYYY-MM-DD_<descrizione>.sql` — applicati manualmente via `sqlcmd` o MCP `db-schema` |

### Submoduli Git

Il progetto include `dr-mcp-dbschema` come submodulo in `tools/mcp-db-schema/`. Dopo ogni pull:

```bash
git submodule update --init --recursive
```

---

## Dati sensibili e configurazione locale

| File | Stato Git | Contiene |
|---|---|---|
| `appsettings.json` | Committato | Solo placeholder |
| `appsettings.local.json` | **Non committato** | Connection string, Jwt:Secret, AllowedOrigins reali |
| `.mcp.json` | **Non committato** | Configurazione MCP con connection string |
| `.mcp.example.json` | Committato | Template con placeholder |

Regola: nessun valore reale va in file committati. Mai. Vedi `.github/instructions/sensitive-data.instructions.md`.

---

## Dove leggere dopo

| Documento | Cosa risponde |
|---|---|
| [`docs/authentication.md`](authentication.md) | Come funziona JWT, localStorage, navigation guard, endpoint protetti |
| [`docs/architecture-backend-findings.md`](architecture-backend-findings.md) | Architettura backend, cronologia decisioni, anti-pattern risolti |
| [`docs/architecture-frontend-findings.md`](architecture-frontend-findings.md) | Architettura frontend, feature aggiunte per sessione |
| [`docs/architecture-backend-plan.md`](architecture-backend-plan.md) | Backlog backend con stato e KPI |
| [`docs/architecture-frontend-plan.md`](architecture-frontend-plan.md) | Backlog frontend con stato e KPI |
| [`.github/copilot-instructions.md`](../.github/copilot-instructions.md) | Stack obbligatorio e checklist post-generazione |
| [`.github/instructions/minimal-api-architecture.instructions.md`](../.github/instructions/minimal-api-architecture.instructions.md) | Pattern Minimal API — copia e incolla da qui |
| [`http://localhost:5083/scalar/v1`](http://localhost:5083/scalar/v1) | Documentazione interattiva endpoint (solo con backend avviato) |

---

*Revisione v1.0 — 2026-04-04 — claude-sonnet-4-6*
