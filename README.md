# Dr.NutrizioNino

Diario alimentare web. Censisce alimenti con i relativi nutrienti, li compone in ricette e li organizza in simulazioni di giornata confrontabili con il fabbisogno nutrizionale personale. I nutrienti possono essere estratti automaticamente dalla foto di un'etichetta tramite un provider LLM a scelta.

Backend Minimal API .NET 10 con database SQL Server, frontend SPA Vue 3, autenticazione JWT con ruoli `User` e `Admin`.

Per installazione, avvio locale, struttura del codice e convenzioni: [`docs/onboarding.md`](docs/onboarding.md).

## Documentazione

### Schede di progetto

| File | Contenuto |
|------|-----------|
| [docs/card-Dr.NutrizioNino.Api.md](docs/card-Dr.NutrizioNino.Api.md) | Scheda del backend: stack, dipendenze, endpoint group, database, servizi esterni, hosting |
| [docs/card-Dr.NutrizioNino.WebVue.md](docs/card-Dr.NutrizioNino.WebVue.md) | Scheda del frontend: stack, dipendenze, struttura, comandi |
| [docs/onboarding.md](docs/onboarding.md) | Guida di ingresso: stack, avvio, struttura del codice, convenzioni, flusso di lavoro |
| [docs/authentication.md](docs/authentication.md) | Flusso JWT, tokenStorage, navigation guard, endpoint protetti |

### Endpoint API

| File | Contenuto |
|------|-----------|
| [docs/endpoint-foods.md](docs/endpoint-foods.md) | Alimenti: CRUD, dashboard, nomi simili, estrazione nutrienti da immagine |
| [docs/endpoint-recipes.md](docs/endpoint-recipes.md) | Ricette: CRUD, ricalcolo nutrizionale, riscalamento del peso |
| [docs/endpoint-brands.md](docs/endpoint-brands.md) | Marche: CRUD, verifica utilizzo, clonazione |
| [docs/endpoint-nutrients.md](docs/endpoint-nutrients.md) | Nutrienti: CRUD, riordino, alias riconosciuti dall'AI |
| [docs/endpoint-units-of-measures.md](docs/endpoint-units-of-measures.md) | Unità di misura: CRUD |
| [docs/endpoint-supermarkets.md](docs/endpoint-supermarkets.md) | Supermercati: CRUD, verifica utilizzo |
| [docs/endpoint-categories.md](docs/endpoint-categories.md) | Categorie merceologiche: CRUD, verifica utilizzo |
| [docs/endpoint-auth.md](docs/endpoint-auth.md) | Login, logout, dati dell'utente autenticato |
| [docs/endpoint-admin.md](docs/endpoint-admin.md) | Gestione utenti riservata agli amministratori |
| [docs/endpoint-user-profile.md](docs/endpoint-user-profile.md) | Storico misurazioni antropometriche dell'utente |
| [docs/endpoint-users.md](docs/endpoint-users.md) | Preferenze di visualizzazione dei grafici |
| [docs/endpoint-nutritional-target.md](docs/endpoint-nutritional-target.md) | Fabbisogno nutrizionale personale |
| [docs/endpoint-daily-simulations.md](docs/endpoint-daily-simulations.md) | Simulazioni di giornata: CRUD, voci, confronto |
| [docs/endpoint-sections.md](docs/endpoint-sections.md) | Sezioni della giornata alimentare |
| [docs/endpoint-vision.md](docs/endpoint-vision.md) | Elenco dei provider LLM disponibili |
| [docs/endpoint-status.md](docs/endpoint-status.md) | Stato del database e retry della connessione |

### Analisi architetturale

| File | Contenuto |
|------|-----------|
| [docs/architecture-backend-findings.md](docs/architecture-backend-findings.md) | Mappa dell'architettura backend, rischi, anti-pattern rilevati |
| [docs/architecture-backend-plan.md](docs/architecture-backend-plan.md) | Backlog e KPI del backend |
| [docs/architecture-frontend-findings.md](docs/architecture-frontend-findings.md) | Mappa dell'architettura frontend, anti-pattern rilevati |
| [docs/architecture-frontend-plan.md](docs/architecture-frontend-plan.md) | Backlog e KPI del frontend |

---

*Documento aggiornato: Luglio 2026 — Revisione v2.0 — 2026-07-29 — claude-opus-5*
