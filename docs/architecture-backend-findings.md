# Backend Architecture Findings

## Executive Summary

Il backend adotta Minimal API con separazione `Endpoints → Services → Repository → DbContext`.

I problemi principali di versioning, CRUD incompleto e metadata OpenAPI sono stati risolti nelle prime revisioni (2026-03-02 / 2026-03-26).

Feature aggiunte (2026-04-03): dominio Categorie con relazione M:N `FoodCategory`, gestione utenti admin completa (CRUD + cambio ruolo), autenticazione migrata da cookie httpOnly a localStorage + header Bearer.

Feature aggiunte (2026-04-04): gestione errori DB con 503, rimozione colonna `Calorie` da `Foods` e `Dishes` (ora calcolata nelle view da nutriente "Energia"), FK pre-check DELETE esteso a Categories/Brands/Supermarkets, fix `Foods.BrandId` nullable.

Feature aggiunte (2026-04-06): fix bug concorrenza DbContext in AdminUserService, reorder nutrienti via endpoint dedicato, ordinamento alfabetico dashboard alimenti, nuovo DTO `NutrientReorderItem`.

Rimangono aperti: uniformità `CancellationToken`, logging con redaction, integration test, baseline metriche.

---

## Mappa architettura corrente

| Livello | File/Cartella |
|---------|---------------|
| Entry point | `src/Dr.NutrizioNino.Api/Program.cs` |
| Route handlers | `src/Dr.NutrizioNino.Api/Endopints/` — Foods, Nutrients, Brands, Units, Dishes, Supermarkets, Categories, Auth, Admin, UserProfile |
| Business logic | `src/Dr.NutrizioNino.Api/Services/` — BrandService, CategoryService, DishService, FoodService, NutrientService, SupermarketService, UnitsOfMeasureService, AuthService, AdminUserService |
| Data access | `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository*.cs` — partial class per dominio |
| EF Core context | `src/Dr.NutrizioNino.Api/Infrastructure/DrNutrizioNinoContext.cs` + partial per Dish, Supermarket, Category |
| Configurazioni EF | `src/Dr.NutrizioNino.Api/Infrastructure/Models/Configurations/` |
| Middleware | `Middleware/HttpContextLogger.cs`, `Middleware/ValidatorMiddleware.cs` |
| OpenAPI | `src/Dr.NutrizioNino.Api/Transformers/` |
| DTO condivisi | `Dr.NutrizioNino.Models/Dto/` — progetto separato |
| Test backend | `src/Testing/Dr.NutrizioNino.Api.Test/` |
| Migrazioni SQL | `schema-migrations/` — file `.sql` con script UP e commenti DOWN |

---

## Stato dei rischi

| ID | Area | Evidenza | Impatto | Stato |
|----|------|----------|---------|-------|
| B01 | API versioning | Route group `api/v{version:apiVersion}/...` su tutti gli endpoint | Alto | ✅ Risolto |
| B02 | Sicurezza request | `ValidatorMiddleware` controlla `/api` — allineato al prefisso reale | Alto | ✅ Risolto |
| B03 | CORS | Policy include `PUT`, `DELETE`. Non richiede più `AllowCredentials` (JWT via header) | Alto | ✅ Risolto |
| B04 | Integrità funzionale | CRUD completo per tutti i domini. Validazione duplicati e FK protection | Alto | ✅ Risolto |
| B05 | Async/EF consistency | Alcuni metodi repository mancano di `CancellationToken` | Medio | ⚠️ Parziale |
| B06 | Logging sensibilità | Header filtrati. `EnableSensitiveDataLogging` ancora attivo in dev | Medio | ⚠️ Parziale |
| B08 | Test coverage | Integration test presenti ma con copertura minima | Alto | ⚠️ Aperto |
| B09 | Metriche baseline | Nessuna misura P95/query-per-request | Medio | ⚠️ Aperto |

---

## Funzionalità aggiunte

### Sessioni 2026-03-25 / 2026-03-26

| ID | Area | Descrizione |
|----|------|-------------|
| B11 | Dominio Piatti | `DishEndpoints`, `DishService`, `DrRepository.Dish`. Calcolo nutrienti proporzionale a 100g. Tabella `DishIngredients` come distinta base storica |
| B13 | Separazione Dishes | Tabella `Dishes` separata da `Foods`. Flag `IsDish` rimosso. View `Foods_Dashboard` aggiornata |
| B14 | Dettaglio piatto | `GET /api/v1/dishes/{id}` → `DishDetailDto` con ingredienti e nutrienti |
| B15 | Dominio Supermercati | Tabella `Supermarkets` + join `FoodSupermarket` (M:N). CRUD con 409 su nome duplicato. 9 catene italiane in seed |

### Sessione 2026-04-06

| ID | Area | Descrizione |
|----|------|-------------|
| B28 | Fix DbContext threading | `AdminUserService.GetUsersAsync`: rimosso `Task.WhenAll(adminsTask, usersTask)`. Le due chiamate a `UserManager.GetUsersInRoleAsync` ora sono await sequenziali — `UserManager` usa lo stesso `DbContext` Scoped e non supporta operazioni concorrenti |
| B29 | Reorder nutrienti | Nuovo endpoint `PUT /api/v1/nutrients/reorder` (AdminOnly). Accetta `IList<NutrientReorderItem>`. `DrRepository.Nutrients`: `GetMaxNutrientPositionOrderAsync` + `ReorderNutrientsAsync` (update batch). Nuovo DTO `NutrientReorderItem(Guid Id, int PositionOrder)` in `Dr.NutrizioNino.Models.Dto` |
| B30 | Nuovo nutriente in ultima posizione | `NutrientService.CreateNutrientAsync`: recupera `MAX(PositionOrder)` dal repository prima della creazione e assegna `PositionOrder = max + 1` automaticamente, ignorando il valore passato dal client |
| B31 | Ordinamento dashboard alimenti | `DrRepository.Food.GetFoodsDashboardAsync`: aggiunto `.OrderBy(f => f.Name)` alla query — l'ordinamento avviene a livello SQL |

### Sessione 2026-04-04

| ID | Area | Descrizione |
|----|------|-------------|
| B23 | Gestione errori DB | `DatabaseExceptionHandler` (IExceptionHandler): intercetta `SqlException` e `InvalidOperationException` con messaggi SQL-correlati, restituisce 503 `"Base Dati non pronta"` invece di 500 |
| B24 | Schema Foods | Colonna `Calorie` rimossa dalla tabella `Foods`. View `Foods_Dashboard` aggiornata: calcola `Calorie` via subquery su `Foods_Nutrients JOIN Nutrients WHERE Name = 'Energia'`. Entità, DTO e repository aggiornati |
| B25 | Schema Dishes | Colonna `Calorie` rimossa dalla tabella `Dishes`. View `Dishes_Dashboard` aggiornata con lo stesso pattern. `DishDetailDto`, `DishService`, `DrRepository.Dish` aggiornati |
| B26 | FK protection DELETE | Pre-check DELETE esteso a Categories, Brands e Supermarkets (pattern già in uso per Nutrients e Units): ritorna 409 Conflict se il record è referenziato da Foods |
| B27 | BrandId nullable | `Foods.BrandId` era `NOT NULL` nel DB ma `Guid?` nell'entità. Allineato con `ALTER TABLE Foods ALTER COLUMN BrandId uniqueidentifier NULL` |

### Sessione 2026-04-03

| ID | Area | Descrizione |
|----|------|-------------|
| B19 | Autenticazione → localStorage | Login restituisce `{ token, userName, role }`. Il token è gestito dal frontend via localStorage + header `Authorization: Bearer`. Rimossa gestione cookie server-side |
| B20 | Dominio Categorie | Entità `Category`, join `FoodCategory` (M:N analoga a Supermarkets). `CategoryService`, `CategoriesEndpoints` CRUD + `is-in-use`. `FoodInfo` esteso con `CategoryIds`. Migration `2026-04-03_categories.sql` |
| B21 | Admin utenti CRUD | `AdminEndpoints`: GET lista, GET per id, POST crea, PUT aggiorna (username/email/data nascita), DELETE, PATCH cambio ruolo. Tutti protetti da `AdminOnly`. `AdminUserService` con Identity `UserManager` |
| B22 | Nutrienti migliorati | `CreateNutrientDto` esteso con `DefaultUnitOfMeasureId`, `PositionOrder`, `DefaultQuantity`. `ModelsFactory.CreateNutrient` li applica |

---

## Bug corretti

| ID | Area | Fix |
|----|------|-----|
| B10 | Precisione DB | `Foods_Nutrients.Quantity`: `numeric(4,2)` → `numeric(6,2)` |
| B17 | FoodNutrient tracking | `contributions` dict re-keyed da `(NutrientId, UomId)` a `NutrientId` |
| B18 | BrandId NOT NULL su Dish | Risolto separando `Dishes` in tabella propria |
| B28 | DbContext concurrency in AdminUserService | `Task.WhenAll` su `GetUsersInRoleAsync` causava `InvalidOperationException`. Fix: await sequenziale |

---

## Anti-pattern risolti

| Anti-pattern | Risolto con |
|--------------|-------------|
| Route non versionate | Route group `api/v{version:apiVersion}/...` su tutti i domini |
| CRUD repository incompleto | Tutti i metodi CRUD implementati |
| Metadata OpenAPI mancante | `Produces`, `WithName`, `WithSummary` uniformi |
| Nessuna validazione duplicati | Check nome/abbreviazione su create/update per tutti i domini |
| Delete senza FK protection | 409 Conflict se record in uso (tutti i domini: Nutrients, UdM, Supermarkets, Categories, Brands) |
| God Table (`IsDish` flag su Foods) | Tabella `Dishes` separata |
| Header autenticazione loggati | `HttpContextLogger` filtra `authorization`, `cookie`, `set-cookie` |

## Anti-pattern ancora presenti

- Metodi `async` repository senza `CancellationToken`.
- `EnableSensitiveDataLogging` attivo in `Program.cs` (dev).
- Test di integrazione con copertura insufficiente.

---

*Ultima revisione: 2026-04-06 — modello `claude-sonnet-4-6`*
