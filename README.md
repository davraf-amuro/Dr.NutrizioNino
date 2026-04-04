# Dr.NutrizioNino

Applicazione web per tenere il Diario Alimentare. Gestisce alimenti, nutrienti, unità di misura, marche e piatti.

## Stack tecnologico

| Layer | Tecnologia |
|-------|------------|
| Backend | .NET 10 · C# 14 · Minimal API · EF Core 10 |
| Database | SQL Server |
| Autenticazione | ASP.NET Core Identity · JWT Bearer · localStorage |
| Frontend | Vue 3 · TypeScript · Vite 8 · Naive UI |
| HTTP client | Axios |
| Logging | Serilog (console + file rotante) |
| API docs | Scalar + OpenAPI |
| Testing | xUnit (integration) |

## Prerequisiti

- .NET 10 SDK
- Node.js 20+
- SQL Server accessibile
- Database `DrNutrizioNino` creato e raggiungibile

## Avvio rapido

### 1. Clona il repository

```bash
git clone <url-repo>
cd Dr.NutrizioNino
git submodule update --init --recursive
```

### 2. Configura la connection string

Crea `src/Dr.NutrizioNino.Api/appsettings.local.json` (non viene committato):

```json
{
  "ConnectionStrings": {
    "DrNutrizioNinoSql": "Data Source=<server>;Initial Catalog=DrNutrizioNino;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
  }
}
```

### 3. Avvia il backend

```bash
cd src/Dr.NutrizioNino.Api
dotnet run
```

API disponibile su `http://localhost:5083`.
Documentazione Scalar: `http://localhost:5083/scalar/v1`

### 4. Avvia il frontend

```bash
cd src/Dr.NutrizioNino.WebVue
npm install
npm run dev
```

Frontend disponibile su `http://localhost:5173`.

### Avvio full-stack con VSCode

Usa la configurazione **Full Stack (API + Vue)** nel pannello *Run & Debug* (F5).

---

## Struttura del progetto

```
Dr.NutrizioNino/
├── src/
│   ├── Dr.NutrizioNino.Api/           # Backend Minimal API
│   │   ├── Endopints/                 # Route handlers (extension methods)
│   │   ├── Infrastructure/            # DbContext, Repository, EF Models
│   │   ├── Services/                  # Business logic (DrService)
│   │   ├── Middleware/                # HttpContextLogger, ValidatorMiddleware
│   │   └── Transformers/              # OpenAPI document transformers
│   └── Dr.NutrizioNino.WebVue/        # Frontend Vue 3
│       └── src/
│           ├── components/            # Componenti UI per dominio
│           ├── modules/*/api/         # Chiamate HTTP per feature (foods, brands, nutrients, units, dishes)
│           ├── modules/*/composables/ # State management per feature
│           ├── views/                 # Pagine principali
│           └── core/                  # useAsyncState, apiClient, ApiError
├── Dr.NutrizioNino.Models/            # DTO condivisi tra API e frontend
├── tools/mcp-db-schema/               # MCP Server per lettura schema DB
├── davraf-guidelines/                 # Submodule linee guida team
├── docs/                              # Documenti architetturali
└── src/Testing/                       # Test di integrazione xUnit
```

---

## Variabili d'ambiente

### Backend

| Chiave | File | Descrizione |
|--------|------|-------------|
| `ConnectionStrings:DrNutrizioNinoSql` | `appsettings.local.json` | Connection string SQL Server |
| `Jwt:Secret` | `appsettings.local.json` | Chiave segreta HMAC-SHA256 per la firma del JWT |
| `AllowedOrigins` | `appsettings.local.json` | Origini CORS ammesse (es. `http://localhost:5173`) |

### Frontend

| Variabile | File | Valore default (dev) |
|-----------|------|----------------------|
| `VITE_API_BASE_URL` | `.env.development` | `http://localhost:5083/api/v1` |

---

## API

Le API sono versionate via URL segment: `/api/v{version}/...`
Versione attiva: **v1**

| Risorsa | Base path |
|---------|-----------|
| Alimenti | `/api/v1/foods` |
| Nutrienti | `/api/v1/nutrients` |
| Unità di misura | `/api/v1/unitsOfMeasures` |
| Marche | `/api/v1/brands` |
| Piatti | `/api/v1/dishes` |
| Supermercati | `/api/v1/supermarkets` |
| Categorie | `/api/v1/categories` |
| Autenticazione | `/api/v1/auth` |
| Amministrazione utenti | `/api/v1/admin/users` |

Documentazione interattiva completa: `http://localhost:5083/scalar/v1`

---

## Autenticazione

Il sistema usa JWT salvato in **localStorage** e inviato come header `Authorization: Bearer <token>` a ogni richiesta via interceptor Axios. Il token è firmato con HMAC-SHA256 e ha una durata di 8 ore.

Due ruoli disponibili: `User` e `Admin`. Gli endpoint amministrativi richiedono la policy `AdminOnly`.

Documentazione dettagliata: [`docs/authentication.md`](docs/authentication.md)

---

## Documentazione

| Documento | Descrizione |
|-----------|-------------|
| [`docs/onboarding.md`](docs/onboarding.md) | Guida di ingresso per sviluppatori — stack, avvio, struttura, convenzioni |
| [`docs/authentication.md`](docs/authentication.md) | Flusso JWT, tokenStorage, navigation guard, endpoint protetti |
| [`docs/architecture-backend-findings.md`](docs/architecture-backend-findings.md) | Mappa architettura backend, rischi, funzionalità aggiunte, anti-pattern |
| [`docs/architecture-backend-plan.md`](docs/architecture-backend-plan.md) | Backlog e KPI del backend |
| [`docs/architecture-frontend-findings.md`](docs/architecture-frontend-findings.md) | Mappa architettura frontend, funzionalità aggiunte, anti-pattern |
| [`docs/architecture-frontend-plan.md`](docs/architecture-frontend-plan.md) | Backlog e KPI del frontend |

---

## MCP Server

Il progetto include `dr-mcp-dbschema`, un MCP Server che permette agli agenti AI di leggere lo schema del database SQL Server ed eseguire DDL con autorizzazione a tre livelli (🟢 CREATE / 🟡 ALTER struttura esistente / 🔴 DROP o ALTER COLUMN).

Configurato in `.mcp.json`. Vedi [`tools/mcp-db-schema/README.md`](tools/mcp-db-schema/README.md) per i dettagli.

---

*Aggiornato il: 2026-04-04*
