# Piano: vision-p1p2-fix

**Data:** 2026-06-20
**Stato:** IN CORSO

## Obiettivo

Risolvere due problemi nella feature AI Vision di riconciliazione nutrienti:
1. **P1 — Sale IncompleteMatch**: l'abbreviazione "mg" nel DB è usata per microgrammi ma l'AI usa "mg" = milligrammi (ISO). Migration DB necessaria.
2. **P2a — availableNutrients vuota**: `FoodsView.vue` non passa `:available-nutrients` a `FoodDetail`. Drop silenzioso: prop opzionale, default `[]`.
3. **P2b — NutrientQuickAddModal**: bottone inline per creare nutriente sconosciuto direttamente dal componente di riconciliazione.

## Scope

**File da modificare:**
- DB: `dbo.UnitsOfMeasures` + `dbo.Nutrients` (UPDATE tramite sqlcmd)
- `src/Dr.NutrizioNino.WebVue/src/modules/foods/composables/useFoods.ts`
- `src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue`
- `src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue`
- `src/Dr.NutrizioNino.WebVue/src/components/Foods/ExtractionReconciliation.vue`

**File da creare:**
- `src/Dr.NutrizioNino.WebVue/src/components/Foods/NutrientQuickAddModal.vue`

**Perimetro negativo:**
- Nessuna modifica a endpoint .NET o DTO C# (il BE già auto-computa `PositionOrder = MAX+1`)
- Nessuna modifica a `NutrientService.cs`, `NutrientsEndpoints.cs`
- Nessuna modifica a `NutrientDto.ts` o `Nutrient.ts`

## Fasi

### Fase 1 — Fix UnitConversionService (P1 — causa reale)

DB già corretto: `mg=Milligrammi`, `mcg=Microgrammi`. La vera causa è che `UnitConversionService.cs` non ha le conversioni:
- "mg" ↔ "gr" (Sale ha UnitaMisura="gr", AI restituisce "mg")
- "g" ↔ "gr" (normalizzazione alias grammi)
- "mcg" ↔ "mg" (DB usa "mcg", service usa "µg")
- "mcg" ↔ "g"/"gr" (cross-domain)

- [x] 1a. Verifica DB: `UnitsOfMeasures` già ok (confermato sqlcmd)
- [x] 1b. Aggiungere conversioni mancanti a `UnitConversionService.cs` (mg↔gr, g↔gr, mcg↔mg, mcg↔g/gr)

### Fase 2 — Bug P2a: prop `availableNutrients` mai passata
- [x] 2a. `useFoods.ts`: aggiungere `nutrients: ref<Nutrient[]>([])`, fetch in `loadLookups`, `addNutrientLookup`, export entrambi
- [x] 2b. `FoodsView.vue`: destructure `nutrients` + `addNutrientLookup`, passare `:available-nutrients="nutrients"` + `@nutrient-created="addNutrientLookup"` a `foodDetail`

### Fase 3 — Feature P2b: NutrientQuickAddModal
- [x] 3a. Creare `NutrientQuickAddModal.vue`
- [x] 3b. `ExtractionReconciliation.vue`: prop Nutrient[], bottone "+ Nuovo", modal, handler
- [x] 3c. `FoodDetail.vue`: prop Nutrient[], emit nutrient-created, onNutrientCreated

### Fase 4 — Lint gate
- [x] 4. `npm run lint` (FE) → exit 0 | `dotnet format --verify-no-changes` (BE) → exit 0

**Stato: COMPLETATO**

---

## Piano: unit-conversions-to-db — COMPLETATO 2026-06-20

File creati/modificati:
- `schema-migrations/2026-06-20_unit-conversions.sql` — CREATE TABLE + seed 20 righe (sqlcmd applicato)
- `Infrastructure/Models/UnitConversion.cs` — entity
- `Infrastructure/Models/Configurations/UnitConversionConfiguration.cs` — EF config
- `Infrastructure/DrNutrizioNinoContext.UnitConversion.cs` — DbSet
- `Infrastructure/DrNutrizioNinoContext.Dish.cs` — ApplyConfiguration aggiunto
- `Services/UnitConversionService.cs` — da static a Singleton+IDbContextFactory+async
- `Services/VisionExtractionService.cs` — inject UnitConversionService, await ConvertAsync
- `Program.cs` — AddDbContextFactory (Singleton) + AddSingleton&lt;UnitConversionService&gt;

Verifica: 20 righe DB ✓ | lint exit 0 ✓

## Criteri di verifica

- `SELECT` sul DB mostra `Abbreviation = 'mcg'` per Sale dopo la migration
- Apertura "Nuovo alimento" → sezione riconciliazione AI → dropdown nutrienti NON è vuoto
- Bottone "Aggiungi nutriente" visibile su Unrecognized → crea nutriente → appare in fondo alla lista nutrienti nel form
- Lint: exit 0

## Note chiave

- `NutrientService.CreateNutrientAsync` auto-computa `PositionOrder = MAX+1` (riga 31-32) — il FE NON deve passarlo
- `Nutrient` (da `nutrients.api.ts`) ha `{ id, name, positionOrder, defaultUnitOfMeasureId, defaultQuantity }` — usare questo al posto di `NutrientDto` per la prop `availableNutrients`
- Il modal `NutrientQuickAddModal` va in `ExtractionReconciliation.vue` (dove è il bottone), non in `FoodDetail.vue`
- `FoodNutrientDto` per il nuovo nutriente in `localFood`: `{ nutrientId, name, positionOrder: MAX+1, unitOfMeasureId: nutrient.defaultUnitOfMeasureId, quantity: 0 }`
