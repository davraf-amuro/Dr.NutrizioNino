# Card: Dr.NutrizioNino.Api

**Minimal API** che espone il diario alimentare Dr.NutrizioNino: alimenti, nutrienti, ricette, simulazioni giornaliere ed estrazione nutrienti da foto di etichette tramite provider LLM.
Espone 16 endpoint group: `Foods`, `Recipes`, `Brands`, `Nutrients`, `Units Of Measures`, `Supermarkets`, `Categories`, `Auth`, `Admin`, `UserProfile`, `Users`, `NutritionalTarget`, `DailySimulations`, `Sections`, `Vision`, `Status`.

## Identificazione
- **Progetto:** Dr.NutrizioNino.Api
- **Solution:** Dr.NutrizioNino.sln
- **Workspace:** —
- **Repository:** https://github.com/davraf-amuro/Dr.NutrizioNino
- **Tipo Applicazione:** Minimal API (.NET 10)
- **Pattern Architetturale:** Minimal API + Service + Repository + Scalar
- **Versione Corrente:** v1 (API version)
- **Owner/Team:**
- **Referente:** Davide Raffagli
- **Contatto Supporto:**

## Stack Tecnologico
- **Linguaggio Principale:** C# 14
- **Framework:** .NET 10 ASP.NET Core Minimal API
- **Target Framework:** net10.0
- **SDK Version:** Microsoft.NET.Sdk.Web — SDK pinned a `10.0.100` (`global.json`, rollForward `latestMinor`)

## Endpoint Groups

| Group | File Mapping | Route Base | Tag Scalar |
|-------|--------------|------------|------------|
| `Foods` | `Endpoints/FoodEndpoints.cs`, `Endpoints/FoodVisionMapping.cs` | `api/v1/foods` | Foods |
| `Recipes` | `Endpoints/RecipeEndpoints.cs` | `api/v1/recipes` | Recipes |
| `Brands` | `Endpoints/BrandsEndpoints.cs` | `api/v1/brands` | Brands |
| `Nutrients` | `Endpoints/NutrientsEndpoints.cs`, `Endpoints/NutrientAliasMapping.cs` | `api/v1/nutrients` | Nutrients |
| `Units Of Measures` | `Endpoints/UnitsOfMeasureEndpoints.cs` | `api/v1/unitsOfMeasures` | Units Of Measures |
| `Supermarkets` | `Endpoints/SupermarketsEndpoints.cs` | `api/v1/supermarkets` | Supermarkets |
| `Categories` | `Endpoints/CategoriesEndpoints.cs` | `api/v1/categories` | Categories |
| `Auth` | `Endpoints/AuthEndpoints.cs` | `api/v1/auth` | Auth |
| `Admin` | `Endpoints/AdminEndpoints.cs` | `api/v1/admin/users` | Admin |
| `UserProfile` | `Endpoints/UserProfileEndpoints.cs` | `api/v1/users/me/profile` | UserProfile |
| `Users` | `Endpoints/UserPreferencesMapping.cs` | `api/v1/users/me` | Users |
| `NutritionalTarget` | `Endpoints/NutritionalTargetEndpoints.cs` | `api/v1/users/me/nutritional-target` | NutritionalTarget |
| `DailySimulations` | `Endpoints/DailySimulationEndpoints.cs` | `api/v1/daily-simulations` | DailySimulations |
| `Sections` | `Endpoints/DailySimulationSectionEndpoints.cs` | `api/v1/sections` | Sections |
| `Vision` | `Endpoints/VisionProvidersMapping.cs` | `api/v1/vision` | Vision |
| `Status` | `Endpoints/HealthMapping.cs` | `api/v1/status` | Status |

## Dipendenze

### Progetti Interni
- `Dr.NutrizioNino.Models` — DTO condivisi tra API e frontend

### Pacchetti Esterni
| Pacchetto | Versione | Scopo |
|-----------|----------|-------|
| Asp.Versioning.Http | 10.0.0 | Versionamento API via URL segment |
| Asp.Versioning.Mvc.ApiExplorer | 10.0.0 | Esposizione delle versioni a OpenAPI |
| EfCore.SchemaCompare | 10.0.0 | Confronto modello EF ↔ schema database |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.9 | Validazione token JWT |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.9 | Utenti, ruoli e password hashing |
| Microsoft.AspNetCore.OpenApi | 10.0.9 | Generazione documento OpenAPI |
| Microsoft.EntityFrameworkCore.Design | 10.0.9 | Tooling design-time EF Core |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.9 | Provider SQL Server |
| Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.23.0 | Supporto build container da Visual Studio |
| Scalar.AspNetCore | 2.16.4 | UI interattiva di documentazione API |
| Serilog.AspNetCore | 10.0.0 | Logging strutturato |
| Serilog.Sinks.Console | 6.1.1 | Sink console |
| Serilog.Sinks.File | 7.0.0 | Sink file con rolling giornaliero |
| Tinyhelpers.AspNetCore | 4.2.16 | ProblemDetails di default, transformer OpenAPI |

## Database
| Connection String Key | Nome Database | Tipo | Server/Host | Username | Provider/ORM |
|-----------------------|---------------|------|-------------|----------|--------------|
| `ConnectionStrings:DrNutrizioNinoSql` | da `appsettings.local.json` | SQL Server | da `appsettings.local.json` | da `appsettings.local.json` | EF Core 10 |

> Il `DbContext` è registrato via `AddDbContextFactory<DrNutrizioNinoContext>`. In ambiente Development sono attivi `EnableSensitiveDataLogging` e `EnableDetailedErrors`.

## Servizi Esterni
| Tipo | Nome/Endpoint | Protocollo | Autenticazione | Scopo/Descrizione |
|------|---------------|------------|----------------|-------------------|
| LLM vision | Ollama — chiave `Vision:Ollama:Endpoint` | HTTP | nessuna | Estrazione nutrienti da immagine, modello locale |
| LLM vision | Anthropic Claude — chiave `Vision:Claude:ApiKey` | HTTPS | API key | Estrazione nutrienti da immagine |
| LLM vision | Azure OpenAI — chiavi `Vision:Azure:Endpoint`, `Vision:Azure:ApiKey`, `Vision:Azure:DeploymentName` | HTTPS | API key | Estrazione nutrienti da immagine |

## Configurazione e Hosting
- **Entrypoint:** `src/Dr.NutrizioNino.Api/Program.cs`
- **Ambiente Test:** non pubblicato
- **Ambiente Produzione:** non pubblicato
- **URL Scalar (dev):** `http://localhost:5083/scalar` (profilo `http`), `https://localhost:7048` (profilo `https`)

## Documentazione API
- **OpenAPI/Swagger:** Scalar (esposto solo in ambiente Development)
- **Versioning API:** `UrlSegmentApiVersionReader`, formato gruppo `'v'VVV`
- **Versioni Supportate:** v1

## Sicurezza e resilienza
- **Autenticazione:** JWT Bearer HMAC-SHA256, chiave da `Jwt:Secret`, `ClockSkew` 5 minuti
- **Autorizzazione:** policy `AdminOnly` (ruolo `Admin`); ruoli seedati all'avvio
- **CORS:** policy `permitGetPost`, origini da `AllowedOrigins`, credenziali ammesse
- **Rate limiting:** sliding window — `vision` 3 req/min, `aliases` 10 req/min, `food-search` 30 req/min; rifiuto con `429`
- **Avvio degradato:** se il database non è raggiungibile l'API parte comunque; stato su `GET api/v1/status`, retry su `POST api/v1/status/retry-database`

---
*Revisione v1.0 — 2026-07-29 22:24 — claude-opus-5*
