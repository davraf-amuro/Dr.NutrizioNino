# Piano: Precisione decimale FoodNutrient.Quantity 2→3

Data: 2026-07-14
Stato: COMPLETATO

Nota: tabella reale è `Foods_Nutrients` (non `FoodNutrients` come inizialmente scritto nel piano) —
confermato via query `sys.tables`, ALTER eseguito sul nome corretto.

## Obiettivo

Sale estratto da etichetta = 0,107 g/100g, arrotondato oggi a 0,11 g (perdita ~2.8%) perché
`FoodNutrient.Quantity` è `numeric(6,2)` e il campo FE ha `:precision="2"`. Warroom (5/5 concordi,
vedi sintesi in conversazione) raccomanda: aumentare la precisione a 3 decimali, restare in grammi.
NON cambiare unità di misura (opzione A scartata da tutti i ruoli).

## Fatti verificati

- DB: `FoodNutrients.Quantity` è `numeric(6,2)` (confermato via sqlcmd).
- BE: `src/Dr.NutrizioNino.Api/Infrastructure/Models/Configurations/FoodsNutrientConfiguration.cs:18` →
  `entity.Property(e => e.Quantity).HasColumnType("numeric(6, 2)");`
- FE: `src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodNutrientInput.vue:21` → `:precision="2"` (con `:max="9999"` — resta invariato, 4 cifre intere già capienti).
- Pattern `numeric(6,2)` si ripete anche in `DishConfiguration.cs`, `DishIngredientConfiguration.cs`,
  `DishNutrientConfiguration.cs`, `DailySimulationEntryNutrientConfiguration.cs` (trovato da ARCH) —
  **fuori scope oggi**: sono tabelle di dominio diverso (piatti/simulazioni aggregate), il problema
  segnalato dall'utente riguarda solo il valore per-100g di un alimento.
- `NutrientDetail.vue:15` ha anch'esso `:precision="2"` ma su `DefaultQuantity` (dato anagrafico del
  nutriente, non il valore estratto per alimento) — **fuori scope**.
- Nessun validator FluentValidation con vincoli su cifre decimali trovato per `Quantity`.

## Scope

### File da modificare
- [x] DB: `ALTER TABLE FoodNutrients ALTER COLUMN Quantity numeric(7,3)` — via sqlcmd, eseguito subito (non solo script su disco)
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/Configurations/FoodsNutrientConfiguration.cs` — `HasColumnType("numeric(6, 2)")` → `HasColumnType("numeric(7, 3)")`
- [x] `src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodNutrientInput.vue` — `:precision="2"` → `:precision="3"`

Nota scelta `numeric(7,3)` (non `(6,3)`): mantiene 4 cifre intere (max 9999) invariate rispetto a oggi,
aggiunge solo la terza cifra decimale — nessuna riduzione del range esistente, puramente additivo.

### Perimetro negativo
- Non tocco: `DishConfiguration.cs`, `DishIngredientConfiguration.cs`, `DishNutrientConfiguration.cs`,
  `DailySimulationEntryNutrientConfiguration.cs` (stesso pattern `numeric(6,2)`, ma dominio diverso — se
  serve, è un task separato da aprire dopo verifica scope con l'utente).
- Non tocco: `NutrientDetail.vue` (`DefaultQuantity`, dato anagrafico nutriente).
- Non tocco: unità di misura canonica di Sale o altri nutrienti (opzione A scartata dal warroom).
- Non tocco: `UnitConversions`, `VisionExtractionService.cs`, altre tabelle/entità.
- Non creo migration SQL su file — eseguo l'ALTER direttamente via sqlcmd (feedback utente: eseguire subito, non solo creare file).

## Fasi

### Fase 1: DB — ALTER COLUMN
- **Stato**: [x]
- **Precondizione**: nessun processo con lock lungo su `FoodNutrients` (verificare `sp_who2` se l'ALTER si blocca)
- **File**: nessuno (comando diretto)
- **Operazione**: ALTER
- **Azione**: `sqlcmd -S localhost -d DrNutrizioNino -E -Q "ALTER TABLE FoodNutrients ALTER COLUMN Quantity numeric(7,3)"`
- **Tool ammessi**: Bash (sqlcmd)
- **Verifica passo**: query `sys.columns` conferma `precision=7, scale=3` sulla colonna

### Fase 2: BE — EF configuration
- **Stato**: [x]
- **Precondizione**: Fase 1 completata
- **File**: `src/Dr.NutrizioNino.Api/Infrastructure/Models/Configurations/FoodsNutrientConfiguration.cs`
- **Operazione**: EDIT
- **Azione**: `HasColumnType("numeric(6, 2)")` → `HasColumnType("numeric(7, 3)")` riga 18
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura file, valore aggiornato; `dotnet build` 0 errori

### Fase 3: FE — precision input
- **Stato**: [x]
- **Precondizione**: Fase 2 completata
- **File**: `src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodNutrientInput.vue`
- **Operazione**: EDIT
- **Azione**: `:precision="2"` → `:precision="3"` riga 21 (lasciare `:max="9999"` invariato)
- **Tool ammessi**: nessuno
- **Verifica passo**: rilettura file; `vue-tsc --noEmit` 0 errori

### Fase 4: Verifica finale
- **Stato**: [x]
- **Precondizione**: Fasi 1-3 completate
- **File**: nessuno
- **Operazione**: nessuna
- **Azione**: `dotnet build` API + `vue-tsc --noEmit` FE; query DB conferma precisione colonna
- **Tool ammessi**: Bash
- **Verifica passo**: 0 errori su entrambe le build

## Criteri di verifica finale
- [x] Colonna `FoodNutrients.Quantity` risulta `numeric(7,3)` a DB
- [x] `FoodsNutrientConfiguration.cs` allineato al nuovo tipo colonna
- [x] FE mostra/accetta 3 decimali su quantità alimento (Sale 0,107 non più arrotondato)
- [x] Build API 0 errori, `vue-tsc` 0 errori
- [x] Nessun'altra tabella/file toccato fuori dal perimetro dichiarato
