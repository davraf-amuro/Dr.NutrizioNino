# Piano: Confronto ricette (max 3)
Data: 2026-08-16
Stato: IN CORSO — implementazione completa, verifica runtime pendente

## Obiettivo
Pagina che confronta da 2 a 3 ricette: per ognuna si sceglie ricetta + quantità in grammi, il sistema mostra i nutrienti scalati a quella quantità in una tabella-matrice (valori assoluti).

## Origine
`TODO/20260816-01.md` + esito tavolo warroom del 2026-08-16 (ARCH / BE / UI / UX / DBADMIN).

## Decisioni prese
- **Read-only**: nessuna scrittura su DB. `PATCH {id}/quantity` (`RescaleRecipeAsync`) **non** riusato: muta `WeightGrams` e `Quantity` in modo persistente per tutti gli utenti e arrotonda a 2 decimali a ogni passaggio (drift cumulativo sui micronutrienti).
- **Nessuna migration, nessuna VIEW**: i nutrienti sono già aggregati e persistiti in `Recipes_Nutrients`; il confronto è pura proiezione `Quantity * quantityGrams / WeightGrams`. Verdict DBADMIN: "Soluzione applicativa accettabile".
- **Solo valori assoluti** (decisione utente, 2026-08-16): niente colonna "% obiettivo nutrizionale", niente join con `NutritionalTarget`, niente toggle "per 100 g".
- **Il limite di 3 sta nel validatore, non nel contratto**: `items` è una lista, non tre campi numerati.
- **Una sola query** `WHERE Id IN (@ids)`, non N chiamate a `GetRecipeByIdAsync`.
- **Validazione con `IValidator<T>`** (scelta utente): introdotta l'infrastruttura `Validators/`, prima assente nel progetto.
- **DTO raggruppati** in un solo file per feature, come l'esistente `RecipeDetailDto.cs`.
- **Regole di validazione confermate**: `Items` 2–3 elementi; `RecipeId` non `Guid.Empty` e distinti; `QuantityGrams > 0` e `<= 10000` (cap: `Quantity` è `numeric(6,2)`, max 9999.99).

## Scope

### File creati
- [x] `Dr.NutrizioNino.Models/Dto/RecipeComparisonDto.cs`
- [x] `src/Dr.NutrizioNino.Api/Validators/IValidator.cs`
- [x] `src/Dr.NutrizioNino.Api/Validators/CompareRecipesRequestValidator.cs`
- [x] `src/Dr.NutrizioNino.Api/Recipes.http`
- [x] `src/Dr.NutrizioNino.WebVue/src/Interfaces/recipes/RecipeComparisonDto.ts`
- [x] `src/Dr.NutrizioNino.WebVue/src/modules/recipes/composables/useRecipeComparison.ts`
- [x] `src/Dr.NutrizioNino.WebVue/src/components/Recipes/RecipeComparisonTable.vue`
- [x] `src/Dr.NutrizioNino.WebVue/src/views/RecipeComparisonView.vue`

### File modificati
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository.Recipe.cs` — `GetRecipesForComparisonAsync`
- [x] `src/Dr.NutrizioNino.Api/Services/RecipeService.cs` — `CompareRecipesAsync` + `ScaleNutrients`
- [x] `src/Dr.NutrizioNino.Api/Endpoints/RecipeEndpoints.cs` — `POST compare`
- [x] `src/Dr.NutrizioNino.Api/Program.cs` — registrazione validator
- [x] `src/Dr.NutrizioNino.WebVue/src/modules/recipes/api/recipes.api.ts` — `compareRecipes`
- [x] `src/Dr.NutrizioNino.WebVue/src/router/index.ts` — route `/recipes/compare`
- [x] `src/Dr.NutrizioNino.WebVue/src/App.vue` — voce di menu + `activeKey`

### Perimetro negativo (rispettato, verificato con `git status`)
- `RescaleRecipeAsync`, `RecalculateRecipeAsync`, `UpdateRecipeNutrientsAsync`, `PATCH {id}/quantity` — invariati
- `schema-migrations/` — nessuna migration, VIEW o indice aggiunti
- Dominio `DailySimulation` — non toccato
- `NutritionalTarget` / `UserProfile` — nessun aggancio
- `RecipeDetail.vue`, `RecipeNutritionPreview.vue`, `RecipeBuilder.vue`, `RecipesView.vue` — non modificati
- Nessun `git commit` / `git push`

---

## Fasi

### Fase 1: DTO di confronto — [x]
`RecipeComparisonDto.cs` con `CompareRecipeItem`, `CompareRecipesRequest`, `RecipeComparisonNutrientDto`, `RecipeComparisonItemDto`, `RecipeComparisonDto`.

### Fase 2: infrastruttura di validazione — [x]
`Validators/IValidator.cs` con `IValidator<T>` e `ValidationResult` (`Success` / `Failure`).

### Fase 3: validatore del body — [x]
`CompareRecipesRequestValidator` con le tre regole confermate, chiavi di errore separate per conteggio, id vuoto, duplicati e quantità.

### Fase 4: lettura dati (repository) — [x]
`GetRecipesForComparisonAsync(IReadOnlyList<Guid> ids, ct)`: `AsNoTracking`, `Where(ids.Contains)`, `Include(RecipeNutrients).ThenInclude(Nutrient)`, nessun `Include` sugli ingredienti, nessuna scrittura.

### Fase 5: scaling nel service — [x]
`CompareRecipesAsync` + `ScaleNutrients`: fattore `quantityGrams / WeightGrams` a piena precisione, `Math.Round` solo sul valore finale, `Nutrients` vuota se `WeightGrams <= 0`, `null` se una ricetta non esiste, ordinamento per `PositionOrder`.

### Fase 6: endpoint POST compare — [x]
`group.MapPost("compare", ...)` con validazione prima della query, `ValidationProblem` su 400, `ProblemDetails` su 404, `.RequireAuthorization()`, metadata OpenAPI completo.

### Fase 7: registrazione DI — [x]
`AddScoped<IValidator<CompareRecipesRequest>, CompareRecipesRequestValidator>()` in `Program.cs`. Build: 0 errori.

### Fase 8: file .http — [x]
`Recipes.http` con 8 richieste: 200 (2 e 3 ricette), 400 (1 item, 4 item, duplicati, quantità non valida), 404, 401.

### Fase 9: tipi frontend — [x]
`RecipeComparisonDto.ts` allineato ai record backend.

### Fase 10: client API — [x]
`compareRecipes(request)` in `recipes.api.ts`.

### Fase 11: composable — [x]
`useRecipeComparison` con 3 slot, `canCompare` da 2 slot, `hasDuplicates`, `mergedRows` unificati per `nutrientId` con `null` sui nutrienti mancanti, `staleRecipeNames`.

### Fase 12: tabella-matrice — [x]
`RecipeComparisonTable.vue` con colonne generate a runtime, cella `—` sui `null`, massimo di riga marcato con `▲ max` oltre al colore, `aria-label`. Nessuno scroll interno (decisione utente 2026-08-17: scorre già la pagina), quindi niente `max-height`, `scroll-x` né colonna `fixed`.

### Fase 13: pagina di confronto — [x]
`RecipeComparisonView.vue`: griglia responsive di 3 slot (select filtrabile + input grammi), avviso duplicati, avviso `isNutritionStale`, azzeramento slot e reset. Nessuna chiamata `apiClient` diretta.

### Fase 14: route e menu — [x]
Route `/recipes/compare` protetta + voce di menu "Confronto ricette"; `activeKey` valuta `/recipes/compare` prima di `/recipes`.

### Fase 15: lint gate — [x]
`dotnet format --verify-no-changes` → exit 0. `npm run lint` → exit 0. `npm run type-check` (vue-tsc) → exit 0.

---

## Criteri di verifica finale

### Verificati staticamente
- [x] `dotnet build` del progetto API: 0 errori (118 warning tutti preesistenti, nullability su `DbSet` in tutto `DrRepository.*`)
- [x] `dotnet format --verify-no-changes` exit 0
- [x] `npm run lint` exit 0
- [x] `npm run type-check` (vue-tsc) exit 0
- [x] `CompareRecipesAsync` non invoca `RescaleRecipeAsync`, `UpdateRecipeNutrientsAsync` né `RecalculateRecipeAsync`
- [x] `GetRecipesForComparisonAsync` non chiama `SaveChangesAsync` e non include `RecipeIngredients`
- [x] Nessun file del perimetro negativo modificato (`git status`)

### Da verificare a runtime (richiedono API + DB avviati)
- [ ] `POST /api/v1/recipes/compare` risponde 200 con 2 e con 3 ricette
- [ ] Risponde 400 con 1 item, 4 item, `recipeId` duplicati, `quantityGrams <= 0` o `> 10000`
- [ ] Risponde 404 quando un `recipeId` non esiste
- [ ] Risponde 401 senza token
- [ ] Dopo una chiamata a `compare`, `WeightGrams` e `Recipes_Nutrients.Quantity` sono invariati sul DB
- [ ] La tabella FE mostra una riga per nutriente e una colonna per ricetta, con cella vuota (non `0`) dove il nutriente manca
- [ ] Il massimo di riga è distinguibile senza fare affidamento sul solo colore
