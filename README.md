# Dr.NutrizioNino

Applicazione web per la gestione di alimenti, nutrienti, unità di misura e marche.
Backend .NET 10 Minimal API · Frontend Vue 3.

## Stack tecnologico

| Layer | Tecnologia |
|-------|------------|
| Backend | .NET 10 · C# 14 · Minimal API · EF Core 10 |
| Database | SQL Server |
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
│           ├── modules/*/api/         # Chiamate HTTP per feature
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

Documentazione interattiva completa: `http://localhost:5083/scalar/v1`

---

## MCP Server

Il progetto include `dr-mcp-dbschema`, un MCP Server che permette agli agenti AI di leggere lo schema del database SQL Server.

Configurato in `.mcp.json`. Vedi [`tools/mcp-db-schema/README.md`](tools/mcp-db-schema/README.md) per i dettagli.

---

*Aggiornato il: 2026-03-18*
