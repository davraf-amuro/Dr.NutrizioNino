# Piano: Rename dominio Dish → Recipe (Piatto → Ricetta)
Data: 2026-08-15
Stato: COMPLETATO

## Obiettivo
Rinominare l'intero dominio "Dish" in "Recipe" su sorgente (C# + Vue/TS) e database, mantenendo in italiano le sole scritte di interfaccia ("Piatti" → "Ricette").

## Decisioni approvate dall'utente
1. **Rotta API**: rinomina secca `api/v1/dishes` → `api/v1/recipes`. Nessun alias di compatibilità.
2. **File storici**: `schema-migrations/` già applicate, `TODO/`, `.ai/plans/` precedenti e `docs/architecture-*` NON vengono toccati. Si aggiunge una nuova migration.
3. **Strategia DB**: `sp_rename` in-place + DROP/CREATE delle 2 viste. Zero perdita dati.

## Mappa di rinomina

### Database (dbo)
| Attuale | Nuovo |
|---|---|
| tabella `Dishes` | `Recipes` |
| tabella `DishIngredients` | `RecipeIngredients` |
| tabella `Dishes_Nutrients` | `Recipes_Nutrients` |
| vista `Dishes_Dashboard` | `Recipes_Dashboard` |
| colonna `DishIngredients.DishId` | `RecipeId` |
| colonna `Dishes_Nutrients.DishId` | `RecipeId` |
| colonna vista `Foods_Dashboard.IsDish` | `IsRecipe` |
| `PK_Dishes` / `PK_DishIngredients` / `PK_Dishes_Nutrients` | `PK_Recipes` / `PK_RecipeIngredients` / `PK_Recipes_Nutrients` |
| `FK_Dishes_Owner` / `FK_Dishes_UnitsOfMeasures` | `FK_Recipes_Owner` / `FK_Recipes_UnitsOfMeasures` |
| `FK_Dishes_Nutrients_Dishes` / `_Nutrients` / `_UnitsOfMeasures` | `FK_Recipes_Nutrients_Recipes` / `_Nutrients` / `_UnitsOfMeasures` |
| `FK_DishIngredients_Dish` / `FK_DishIngredients_Food` | `FK_RecipeIngredients_Recipe` / `FK_RecipeIngredients_Food` |
| `IX_Dishes_IsNutritionStale` | `IX_Recipes_IsNutritionStale` |
| `IX_DishIngredients_FoodId` | `IX_RecipeIngredients_FoodId` |
| `DF__Dishes__IsNutrit__52E34C9D` | `DF_Recipes_IsNutritionStale` |

### Backend C#
| Attuale | Nuovo |
|---|---|
| `Dish` / `DishIngredient` / `DishNutrient` | `Recipe` / `RecipeIngredient` / `RecipeNutrient` |
| `DishDashboardInfo` | `RecipeDashboardInfo` |
| `DishService` | `RecipeService` |
| `DishEndpoints` / `MapsDishesEndpoints` | `RecipeEndpoints` / `MapsRecipesEndpoints` |
| `CreateDishDto` / `DishDetailDto` / `DishIngredientDto` | `CreateRecipeDto` / `RecipeDetailDto` / `RecipeIngredientDto` |
| `FoodDashboardInfo.IsDish` | `IsRecipe` |
| `DailySimulationSourceType.Dish` (=1) | `.Recipe` (=1, valore invariato) |
| DbSet `Dishes` / `DishNutrients` / `DishIngredients` / `DishesDashboard` | `Recipes` / `RecipeNutrients` / `RecipeIngredients` / `RecipesDashboard` |

### Frontend Vue/TS
| Attuale | Nuovo |
|---|---|
| `src/Interfaces/dishes/` | `src/Interfaces/recipes/` |
| `src/modules/dishes/` | `src/modules/recipes/` |
| `src/components/Dishes/` | `src/components/Recipes/` |
| `DishesView.vue` / `DishBuilder.vue` / `DishDetail.vue` / `DishesList.vue` / `DishIngredientList.vue` / `DishNutritionPreview.vue` | prefisso `Recipe*` |
| `useDishes.ts` / `useDishCalculator.ts` / `dishes.api.ts` | `useRecipes.ts` / `useRecipeCalculator.ts` / `recipes.api.ts` |
| route path `/dishes`, name `dishes` | `/recipes`, `recipes` |

### Scritte UI (restano italiane)
`Piatti` → `Ricette` · `piatto` → `ricetta` · `Nuovo piatto` → `Nuova ricetta` · `Lista piatti` → `Lista ricette`

## Scope

### File da modificare
Backend — entità e configurazioni:
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/Dish.cs` → `Recipe.cs`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/DishIngredient.cs` → `RecipeIngredient.cs`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/DishNutrient.cs` → `RecipeNutrient.cs`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/DishDashboardInfo.cs` → `RecipeDashboardInfo.cs`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/Food.Dish.cs` → `Food.Recipe.cs`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/FoodDashboardInfo.cs` — `IsDish` → `IsRecipe`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/DailySimulationEntry.cs` — enum member
- [x] `.../Configurations/DishConfiguration.cs` → `RecipeConfiguration.cs`
- [x] `.../Configurations/DishIngredientConfiguration.cs` → `RecipeIngredientConfiguration.cs`
- [x] `.../Configurations/DishNutrientConfiguration.cs` → `RecipeNutrientConfiguration.cs`
- [x] `.../Configurations/DishDashboardConfiguration.cs` → `RecipeDashboardConfiguration.cs`
- [x] `.../Configurations/FoodConfiguration.Dish.cs` → `FoodConfiguration.Recipe.cs`

Backend — context, repository, service, endpoint:
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/DrNutrizioNinoContext.Dish.cs` → `DrNutrizioNinoContext.Recipe.cs`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository.Dish.cs` → `DrRepository.Recipe.cs`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository.Food.cs` — chiamata `MarkDishesStaleByFoodIdAsync`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository.DailySimulation.cs`
- [x] `src/Dr.NutrizioNino.Api/Services/DishService.cs` → `RecipeService.cs`
- [x] `src/Dr.NutrizioNino.Api/Services/DailySimulationService.cs`
- [x] `src/Dr.NutrizioNino.Api/Endpoints/DishEndpoints.cs` → `RecipeEndpoints.cs`
- [x] `src/Dr.NutrizioNino.Api/Endpoints/FoodEndpoints.cs` — campo `IsDish`
- [x] `src/Dr.NutrizioNino.Api/Program.cs` — DI + map endpoints

Backend — DTO condivisi:
- [x] `Dr.NutrizioNino.Models/Dto/CreateDishDto.cs` → `CreateRecipeDto.cs`
- [x] `Dr.NutrizioNino.Models/Dto/DishDetailDto.cs` → `RecipeDetailDto.cs`
- [x] `Dr.NutrizioNino.Models/Dto/DishIngredientDto.cs` → `RecipeIngredientDto.cs`
- [x] `Dr.NutrizioNino.Models/Dto/DailySimulationDto.cs`

Frontend:
- [x] `src/Dr.NutrizioNino.WebVue/src/Interfaces/dishes/*` → `Interfaces/recipes/*`
- [x] `src/Dr.NutrizioNino.WebVue/src/Interfaces/dailySimulations/DailySimulationDto.ts`
- [x] `src/Dr.NutrizioNino.WebVue/src/modules/dishes/*` → `modules/recipes/*`
- [x] `src/Dr.NutrizioNino.WebVue/src/core/composables/useDishCalculator.ts` → `useRecipeCalculator.ts`
- [x] `src/Dr.NutrizioNino.WebVue/src/components/Dishes/*` → `components/Recipes/*`
- [x] `src/Dr.NutrizioNino.WebVue/src/components/DailySimulations/DailySimulationDetail.vue`
- [x] `src/Dr.NutrizioNino.WebVue/src/views/DishesView.vue` → `RecipesView.vue`
- [x] `src/Dr.NutrizioNino.WebVue/src/router/index.ts`
- [x] `src/Dr.NutrizioNino.WebVue/src/App.vue`

Database e docs:
- [x] `schema-migrations/2026-08-15_dish-to-recipe-rename.sql` — NUOVO
- [x] `docs/endpoint-dishes.md` → `docs/endpoint-recipes.md`
- [x] `docs/endpoint-daily-simulations.md`
- [x] `docs/endpoint-units-of-measures.md`
- [x] `docs/card-Dr.NutrizioNino.Api.md`
- [x] `docs/card-Dr.NutrizioNino.WebVue.md`
- [x] `docs/onboarding.md`
- [x] `README.md`

### Perimetro negativo
Non toccherò:
- `schema-migrations/*` esistenti (traccia storica immutabile)
- `docs/migrations/*` (storico)
- `TODO/*` e `.ai/plans/` di sessioni precedenti
- `docs/architecture-backend-findings.md`, `architecture-backend-plan.md`, `architecture-frontend-findings.md`, `architecture-frontend-plan.md` (report di audit datati)
- `schema-migrations/2026-04-02_1715_AspNetUsers.sql` e `schema-migrations/2026-08-09_1000_AspNetUsers.sql` (modifiche non committate estranee al task)
- `.agents/`, `AGENTS.md`, `tools/dr-mcp-dbschema/`, `davraf-guidelines/`
- Traduzione in inglese dei commenti XML italiani preesistenti (fuori scope: si limita la modifica a "piatto" → "ricetta")

## Fasi (formato atomico)

### Fase 1: Migration SQL di rename
- **Stato**: [x]
- **Precondizione**: tabelle `Dishes`, `DishIngredients`, `Dishes_Nutrients` e viste `Dishes_Dashboard`, `Foods_Dashboard` esistono su `localhost/DrNutrizioNino`
- **File**: `schema-migrations/2026-08-15_dish-to-recipe-rename.sql`
- **Operazione**: CREATE
- **Azione**: scrivere script idempotente: `sp_rename` per tabelle, colonne, PK, FK, indici e default constraint secondo la mappa DB; `DROP VIEW`/`CREATE VIEW` per `Recipes_Dashboard` (ex `Dishes_Dashboard`) e per `Foods_Dashboard` con colonna `IsRecipe`
- **Tool ammessi**: sqlcmd (read-only in questa fase)
- **Verifica passo**: il file esiste e contiene tutte le 20 voci della mappa DB
- **Su divergenza**: STOP — scrivi `⚠️ Divergenza Fase 1: <cosa>` in plan.md

### Fase 2: Applicazione migration al DB
- **Stato**: [x]
- **Precondizione**: Fase 1 `[x]`; API non in esecuzione
- **File**: nessuno (esecuzione)
- **Operazione**: EDIT (database)
- **Azione**: eseguire lo script con `sqlcmd -S localhost -d DrNutrizioNino -E -C -i <file>`
- **Tool ammessi**: sqlcmd
- **Verifica passo**: query su `sys.objects`/`sys.columns`/`sys.indexes` con `LIKE '%Dish%'` restituisce **0 righe**; query con `LIKE '%Recipe%'` restituisce le 20 voci attese
- **Su divergenza**: STOP — rollback tramite sp_rename inverso

### Fase 3: Backend — entità e configurazioni EF
- **Stato**: [x]
- **Precondizione**: Fase 2 `[x]`
- **File**: i 12 file elencati in "Backend — entità e configurazioni"
- **Operazione**: EDIT + rename file (`git mv`)
- **Azione**: rinominare classi, proprietà, navigation property e nomi tabella/vista/constraint nelle `IEntityTypeConfiguration` secondo la mappa
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura di ogni file; nessuna occorrenza di `Dish` residua nei 12 file
- **Su divergenza**: STOP

### Fase 4: Backend — DbContext, repository, service
- **Stato**: [x]
- **Precondizione**: Fase 3 `[x]`
- **File**: `DrNutrizioNinoContext.Dish.cs`, `DrRepository.Dish.cs`, `DrRepository.Food.cs`, `DrRepository.DailySimulation.cs`, `DishService.cs`, `DailySimulationService.cs`
- **Operazione**: EDIT + rename file
- **Azione**: rinominare DbSet, metodi (`GetDishDetailAsync` → `GetRecipeDetailAsync`, `MarkDishesStaleByFoodIdAsync` → `MarkRecipesStaleByFoodIdAsync`, ecc.), variabili locali e commenti italiani ("piatto" → "ricetta")
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura di ogni file; nessuna occorrenza di `Dish`/`piatt` residua
- **Su divergenza**: STOP

### Fase 5: Backend — DTO condivisi
- **Stato**: [x]
- **Precondizione**: Fase 4 `[x]`
- **File**: i 4 file in `Dr.NutrizioNino.Models/Dto/`
- **Operazione**: EDIT + rename file
- **Azione**: rinominare record DTO e le loro proprietà secondo la mappa
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura; nessuna occorrenza di `Dish` residua
- **Su divergenza**: STOP

### Fase 6: Backend — endpoint e Program.cs
- **Stato**: [x]
- **Precondizione**: Fase 5 `[x]`
- **File**: `DishEndpoints.cs` → `RecipeEndpoints.cs`, `FoodEndpoints.cs`, `Program.cs`
- **Operazione**: EDIT + rename file
- **Azione**: rotta `api/v{version:apiVersion}/recipes`, `WithTags("Recipes")`, tutti i `WithName`/`WithSummary`/`WithDescription`, `Location` header del clone, campo `IsRecipe` in `FoodDashboardResponse`, `AddScoped<RecipeService>()`, `app.MapsRecipesEndpoints(versionSet)`
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura; nessuna occorrenza di `Dish` residua
- **Su divergenza**: STOP

### Fase 7: Verifica build backend
- **Stato**: [x]
- **Precondizione**: Fase 6 `[x]`
- **File**: nessuno
- **Operazione**: verifica
- **Azione**: `dotnet build src/Dr.NutrizioNino.Api/Dr.NutrizioNino.Api.csproj`
- **Tool ammessi**: dotnet CLI
- **Verifica passo**: exit code 0, zero errori di compilazione
- **Su divergenza**: STOP — correggi e riesegui (max 1 ritentativo)

### Fase 8: Frontend — interfacce, moduli, composables, api
- **Stato**: [x]
- **Precondizione**: Fase 7 `[x]`
- **File**: `Interfaces/dishes/*`, `Interfaces/dailySimulations/DailySimulationDto.ts`, `modules/dishes/*`, `core/composables/useDishCalculator.ts`
- **Operazione**: EDIT + rename cartelle/file
- **Azione**: rinominare cartelle, file, tipi, funzioni esportate e path `/dishes` → `/recipes` nelle chiamate `apiClient`
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura; nessuna occorrenza di `dish`/`Dish` residua nei file toccati
- **Su divergenza**: STOP

### Fase 9: Frontend — componenti, view, router, App
- **Stato**: [x]
- **Precondizione**: Fase 8 `[x]`
- **File**: `components/Dishes/*`, `components/DailySimulations/DailySimulationDetail.vue`, `views/DishesView.vue`, `router/index.ts`, `App.vue`
- **Operazione**: EDIT + rename cartelle/file
- **Azione**: rinominare componenti e import; route `/recipes` name `recipes`; **scritte UI tradotte in italiano**: `Piatti` → `Ricette`, `Nuovo piatto` → `Nuova ricetta`, `Nome piatto` → `Nome ricetta`, `Salva piatto` → `Salva ricetta`, `Ingredienti del piatto` → `Ingredienti della ricetta`, `Valori totali del piatto` → `Valori totali della ricetta`, `Lista piatti` → `Lista ricette`, `Eliminare il piatto "X"?` → `Eliminare la ricetta "X"?`, `Seleziona alimento o piatto...` → `Seleziona alimento o ricetta...`
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura; nessuna occorrenza di `dish`/`Dish`/`piatt` residua nei file toccati
- **Su divergenza**: STOP

### Fase 10: Verifica build/lint frontend
- **Stato**: [x]
- **Precondizione**: Fase 9 `[x]`
- **File**: nessuno
- **Operazione**: verifica
- **Azione**: `npm run lint` e `npm run type-check` (o `npm run build`) in `src/Dr.NutrizioNino.WebVue`
- **Tool ammessi**: npm
- **Verifica passo**: exit code 0 per entrambi
- **Su divergenza**: STOP — correggi e riesegui (max 1 ritentativo)

### Fase 11: Documentazione
- **Stato**: [x]
- **Precondizione**: Fase 10 `[x]`
- **File**: `docs/endpoint-dishes.md` → `docs/endpoint-recipes.md`, `docs/endpoint-daily-simulations.md`, `docs/endpoint-units-of-measures.md`, `docs/card-Dr.NutrizioNino.Api.md`, `docs/card-Dr.NutrizioNino.WebVue.md`, `docs/onboarding.md`, `README.md`
- **Operazione**: EDIT + rename file
- **Azione**: aggiornare nomi tabella/entità/rotta/componenti; aggiornare eventuali indici o link interni che puntano a `endpoint-dishes.md`
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura; nessun link rotto a `endpoint-dishes.md`
- **Su divergenza**: STOP

### Fase 12: Verifica finale globale
- **Stato**: [x]
- **Precondizione**: Fase 11 `[x]`
- **File**: nessuno
- **Operazione**: verifica
- **Azione**: `grep -ri "dish\|piatt"` sull'intero repo escludendo il perimetro negativo; `dotnet format src/Dr.NutrizioNino.Api/Dr.NutrizioNino.Api.csproj --verify-no-changes`
- **Tool ammessi**: grep, dotnet CLI
- **Verifica passo**: grep restituisce 0 righe fuori dal perimetro negativo; `dotnet format` exit code 0
- **Su divergenza**: STOP

## Nota scope
Aggiunto in corso d'opera `docs/card-Dr.NutrizioNino.Api-wiki.md` (variante wiki di `card-Dr.NutrizioNino.Api.md`, gia in scope): unica occorrenza "piatti" alla riga 7.

## Criteri di verifica finale
- [x] `sys.objects`/`sys.columns`/`sys.indexes` con `LIKE '%Dish%'` → 0 righe su `DrNutrizioNino`
- [x] Viste `Recipes_Dashboard` e `Foods_Dashboard` (con colonna `IsRecipe`) interrogabili senza errori
- [x] `dotnet build` API → exit code 0
- [x] `dotnet format --verify-no-changes` → exit code 0
- [x] `npm run lint` e `npm run type-check` FE → exit code 0
- [x] `grep -ri "dish\|piatt"` fuori dal perimetro negativo → 0 occorrenze
- [x] Rotta esposta: `api/v1/recipes`; nessun riferimento residuo a `api/v1/dishes`
- [x] Scritte UI in italiano usano "ricetta"/"Ricette"
- [x] `docs/endpoint-recipes.md` esiste; nessun link rotto a `endpoint-dishes.md`
