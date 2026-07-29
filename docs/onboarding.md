# Onboarding — Dr.NutrizioNino

## 1. Il progetto in tre righe

Dr.NutrizioNino è un diario alimentare web. Registra alimenti, marche, categorie, supermercati e nutrienti, li compone in piatti e li organizza in simulazioni di giornata alimentare confrontabili con il fabbisogno personale dell'utente.
Il valore aggiunto è l'estrazione automatica dei nutrienti da una foto dell'etichetta, delegata a un provider LLM scelto dall'utente.
Backend Minimal API .NET 10, frontend SPA Vue 3, database SQL Server.

## 2. Stack e scelte tecniche

| Tecnologia | Versione | Motivo della scelta |
|------------|----------|---------------------|
| .NET | 10 (`net10.0`) | Standard di progetto; SDK pinned in `global.json` a `10.0.100` |
| C# | 14 (`LangVersion 14.0`) | Impostato in `Directory.Build.props`, con `Nullable` e `ImplicitUsings` attivi |
| ASP.NET Core Minimal API | 10 | Nessun Controller: gli endpoint sono extension method in `Endpoints/` |
| EF Core | 10.0.9 (`SqlServer`) | Accesso dati; registrato come `AddDbContextFactory` |
| ASP.NET Core Identity | 10.0.9 | Utenti e ruoli; `ApplicationUser` con chiave `Guid` |
| JWT Bearer | 10.0.9 | Autenticazione stateless; sovrascrive esplicitamente gli schema default di Identity |
| Asp.Versioning | 10.0.0 | Versionamento via URL segment (`api/v1/...`) |
| Scalar | 2.16.4 | UI di documentazione API — sostituisce Swagger UI, esposta solo in Development |
| Serilog | 10.0.0 | Logging strutturato su console e file CompactJson |
| TinyHelpers.AspNetCore | 4.2.16 | `ProblemDetails` di default e transformer OpenAPI |
| Vue | 3.5 | Frontend SPA, Composition API |
| TypeScript | 5.9 | Tipizzazione allineata ai DTO backend |
| Vite | 8 | Dev server e build frontend |
| Naive UI | 2.44 | Libreria componenti |
| Chart.js + vue-chartjs | 4.5 / 5.3 | Grafici nutrienti, con `chartjs-plugin-annotation` per le soglie |
| Axios | 1.13 | Client HTTP con interceptor JWT |
| xUnit | — | Test di integrazione in `src/Testing/Dr.NutrizioNino.Api.Test` |

## 3. Come avviare il progetto

**Prerequisiti:** .NET 10 SDK, Node.js, un'istanza SQL Server raggiungibile con il database `DrNutrizioNino` creato.

**Passo 1 — clone e submodule**

```bash
git clone https://github.com/davraf-amuro/Dr.NutrizioNino.git
cd Dr.NutrizioNino
git submodule update --init --recursive
```

**Passo 2 — configurazione locale del backend**

`src/Dr.NutrizioNino.Api/appsettings.local.json` non è committato. Va creato con almeno:

```json
{
  "ConnectionStrings": {
    "DrNutrizioNinoSql": "Data Source=<server>;Initial Catalog=DrNutrizioNino;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
  },
  "Jwt": { "Secret": "<segreto di almeno 32 caratteri>" },
  "AllowedOrigins": [ "http://localhost:5173" ]
}
```

Per usare l'estrazione da immagine serve anche la sezione `Vision` con endpoint e credenziali di almeno un provider (`Ollama`, `Claude`, `Azure`).

**Passo 3 — avvio backend**

```bash
cd src/Dr.NutrizioNino.Api
dotnet run
```

API su `http://localhost:5083`, documentazione Scalar su `http://localhost:5083/scalar`.

Se il database non risponde, l'API parte comunque in stato degradato: `GET /api/v1/status` riporta l'errore, `POST /api/v1/status/retry-database` ritenta connessione e seed dei ruoli.

**Passo 4 — avvio frontend**

```bash
cd src/Dr.NutrizioNino.WebVue
npm install
npm run dev
```

Frontend su `http://localhost:5173`. La base URL dell'API si configura in `.env.development` tramite `VITE_API_BASE_URL`.

**Avvio da VS Code:** `.vscode/launch.json` e `.vscode/tasks.json` sono versionati nel repository. I profili possono essere rigenerati con la skill `/CreateLaunchProfiles`.

## 4. Struttura del codice

```
Dr.NutrizioNino/
├── src/
│   ├── Dr.NutrizioNino.Api/        # Backend Minimal API
│   ├── Dr.NutrizioNino.WebVue/     # Frontend Vue 3
│   ├── Infrastructure/             # Dr.NutrizioNino.Provider.Sql
│   └── Testing/                    # Test di integrazione xUnit
├── Dr.NutrizioNino.Models/         # DTO condivisi tra API e frontend
├── docs/                           # Documentazione generata
├── schema-migrations/              # Script SQL di evoluzione schema
├── tools/dr-mcp-dbschema/          # MCP server per la lettura dello schema DB
├── davraf-guidelines/              # Submodule linee guida
└── .ai/plans/                      # Piani di lavoro persistiti
```

**Backend — `src/Dr.NutrizioNino.Api/`**

| Cartella | Cosa contiene |
|----------|---------------|
| `Endpoints/` | Un file per gruppo di route, ognuno con un extension method `Map*Endpoints` invocato da `Program.cs`. È qui che si aggiunge un endpoint |
| `Services/` | Logica applicativa, una classe per dominio. Gli handler iniettano il Service, mai il livello dati |
| `Services/Vision/` | `IVisionProvider` e le tre implementazioni (Ollama, Claude, Azure) più la factory |
| `Infrastructure/` | `DrNutrizioNinoContext` e `DrRepository`, entrambi divisi in file parziali per dominio |
| `Infrastructure/Models/` | Entità EF Core |
| `Infrastructure/Models/Configurations/` | `IEntityTypeConfiguration` per ogni entità — il mapping vive qui, non nel `DbContext` |
| `Infrastructure/Extensions/` | Metodi di proiezione entità → DTO |
| `Middleware/` | `HttpContextLogger`, `DatabaseExceptionHandler` |
| `Transformers/` | Transformer OpenAPI (`AddDocumentInformations`, `AddHeaders`, `AddGenericsInformations`) |
| `Helpers/` | `ClaimsPrincipalExtensions.GetUserId()`, costanti, mapper |

**Frontend — `src/Dr.NutrizioNino.WebVue/src/`**

| Cartella | Cosa contiene |
|----------|---------------|
| `modules/<feature>/api/` | Chiamate HTTP di una feature |
| `modules/<feature>/composables/` | Stato e logica riusabile della feature |
| `components/<Dominio>/` | Componenti UI raggruppati per dominio |
| `Interfaces/<dominio>/` | Tipi TypeScript allineati ai DTO backend |
| `core/http/` | `apiClient` Axios, interceptor JWT, `ApiError` |
| `core/composables/` | Composable trasversali, es. `useAsyncState` |
| `views/` e `router/` | Pagine e route con navigation guard |

## 5. Convenzioni obbligatorie

Fonte: `.github/copilot-instructions.md` e i file in `.github/instructions/`.

| Regola | Dove è definita |
|--------|-----------------|
| Endpoint solo come extension method in `Endpoints/`, mai Controller | `minimal-api-architecture.instructions.md` |
| Route sempre `api/v{version:apiVersion}/{gruppo}/{comando?}`, con `WithTags` + `WithApiVersionSet` + `MapToApiVersion` | `minimal-api-architecture.instructions.md` |
| Metadata OpenAPI completi su ogni endpoint: `WithSummary`, `WithDescription`, `WithName`, `Produces<T>` per ogni risultato | `minimal-api-architecture.instructions.md` |
| Handler iniettano il Service, mai direttamente il livello dati | `minimal-api-architecture.instructions.md` |
| Primary constructor e `async`/`await` su ogni I/O | `copilot-instructions.md` |
| Logging strutturato con placeholder, mai interpolazione di stringa nei log | `copilot-instructions.md`, `logging.instructions.md` |
| Naming: namespace `snake_case`, classi `PascalCase`, variabili `camelCase` | `copilot-instructions.md` |
| Un tipo per file, record inclusi | `code-organization.instructions.md` |
| Nessun valore letterale hardcoded: centralizzare in costanti o configurazione | `no-hardcoded-values.instructions.md` |
| Credenziali mai in file committati; `appsettings.local.json` è in `.gitignore` | `sensitive-data.instructions.md` |
| Task con ≥ 2 operazioni: piano su disco in `.ai/plans/<YYYY-MM-DD>-<slug>/` prima di agire | `plan-tracking.instructions.md` |
| Ogni documento in `docs/` chiude con `*Revisione vN — YYYY-MM-DD HH:MM — modello*` | `doc-versioning.instructions.md` |
| Ogni regola condivisa deve funzionare sia con Claude Code sia con GitHub Copilot | `CLAUDE.md` |

**Divergenze note tra istruzioni e codice esistente.** Vale la pena saperlo prima di aprire una PR:

- `minimal-api-architecture.instructions.md` prescrive un Provider con `Filter.ToExpression()` e DTO con `static Projection`. Il codice usa invece `DrRepository`, diviso in file parziali per dominio, con proiezioni negli extension method di `Infrastructure/Extensions/`.
- La stessa istruzione prescrive `IValidator<T>` per ogni body. Nel progetto non esiste alcun validatore: i controlli sono inline negli handler e, dove serve coerenza tra route e body, in un `AddEndpointFilter` (esempio: `PUT /api/v1/foods/{id}`).

Prima di allineare il codice alle istruzioni o viceversa, concorda la direzione: entrambe le scelte impattano tutti i gruppi di endpoint.

## 6. Flusso di lavoro

| Attività | Come si fa |
|----------|-----------|
| Branch di lavoro | Si parte da `dev`. `master` è il branch principale |
| Promozione | Skill `/promote-to <target-branch>` — esegue commit, push e apre la PR |
| Lint .NET (gate di push) | `dotnet format src/Dr.NutrizioNino.Api/Dr.NutrizioNino.Api.csproj --verify-no-changes` |
| Lint frontend (gate di push) | `npm run lint` da `src/Dr.NutrizioNino.WebVue` |
| Type-check frontend | `npm run type-check` |
| Test di integrazione | `dotnet test src/Testing/Dr.NutrizioNino.Api.Test/Dr.NutrizioNino.Api.IntegrationTest.csproj` |
| Aggiornamento linee guida | Skill `/get-latest` — aggiorna il submodule `davraf-guidelines` e propaga i file |

> Regola assoluta: nessun `git push` senza lint pulito. Exit code diverso da zero blocca la push.

**Evoluzione dello schema database.** Il progetto non usa le migration EF Core: gli script SQL stanno in `schema-migrations/` e in `docs/migrations/`, e vanno eseguiti sul database prima di avviare il codice che li richiede.

## 7. Dati sensibili e configurazione locale

| File | Contenuto | Committato |
|------|-----------|------------|
| `src/Dr.NutrizioNino.Api/appsettings.json` | Solo placeholder (`CHISSADOVE`, `CHISSAQUALE`) | Sì |
| `src/Dr.NutrizioNino.Api/appsettings.local.json` | Connection string, `Jwt:Secret`, chiavi dei provider vision | No — in `.gitignore` |
| `src/Dr.NutrizioNino.WebVue/.env.development` | `VITE_API_BASE_URL` | Regolato dal `.gitignore` del frontend |
| `.mcp.json` | Configurazione MCP con percorsi reali | No — usare `.mcp.example.json` come modello |
| `docs/*-wiki.md` | Schede operative con valori reali | No — in `.gitignore` |

Ogni configurazione è sovrascrivibile da variabile d'ambiente: `Program.cs` chiama `AddEnvironmentVariables()` e il separatore di sezione è `__` (esempio: `ConnectionStrings__DrNutrizioNinoSql`).

Regole complete in `.github/instructions/sensitive-data.instructions.md`.

## 8. Dove chiedere / cosa leggere dopo

| Documento | Contenuto |
|-----------|-----------|
| [`CLAUDE.md`](../CLAUDE.md) | Regole di collaborazione con gli agenti AI, invocazione delle skill |
| [`.github/copilot-instructions.md`](../.github/copilot-instructions.md) | Standard di progetto .NET, checklist pre e post generazione |
| [`.github/instructions/`](../.github/instructions/) | Istruzioni modulari: architettura, logging, validazione, dati sensibili, CI/CD |
| [`docs/card-Dr.NutrizioNino.Api.md`](card-Dr.NutrizioNino.Api.md) | Scheda del backend: stack, dipendenze, endpoint group, hosting |
| [`docs/card-Dr.NutrizioNino.WebVue.md`](card-Dr.NutrizioNino.WebVue.md) | Scheda del frontend |
| `docs/endpoint-*.md` | Un documento per gruppo di endpoint, con tabelle e diagrammi di flusso |
| `http://localhost:5083/scalar` | Documentazione API interattiva, disponibile in Development |

Le domande sul dominio nutrizionale (formule, categorie, correttezza dei dati) hanno una skill dedicata: `/nutrizionista`.

---
*Revisione v2.0 — 2026-07-29 22:24 — claude-opus-5*
