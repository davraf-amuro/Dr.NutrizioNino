# Piano: Esporre categoria in griglia Alimenti

Data: 2026-07-20
Stato: COMPLETATO

Nota: la migration iniziale era basata su una versione superata della vista `Foods_Dashboard`
(mancavano `f.OwnerId` e il calcolo `Calorie` via `Foods_Nutrients` introdotti in
`2026-04-04_remove-calorie-from-foods.sql`) — corretta prima dell'esecuzione su DB.
Scope aggiunto durante l'esecuzione: `FoodEndpoints.cs` (record `FoodDashboardResponse` + 2 punti
di mapping), non individuato nell'esplorazione iniziale ma necessario per propagare il campo
all'API.

## Obiettivo

Griglia Alimenti (`FoodsList.vue`) mostra già colonne lookup denormalizzate: `brandDescription` (FK
singola) e `supermarketsText` (M:N, CSV → chip). Categoria è già gestita in editing (`FoodDto.categoryIds`,
form `FoodDetail.vue`) tramite relazione M:N (`Categories` / `FoodCategory`), ma non è proiettata nella
dashboard. Aggiungere colonna "Categorie" replicando esattamente il pattern `SupermarketsText`.

Posizione colonna confermata dall'utente: subito dopo "Marca" (Nome, Marca, Categorie, UdM, Quantità,
Supermercati, Azioni).

## Scope — file da modificare, in ordine

1. `schema-migrations/2026-07-20_foods-dashboard-view-categories.sql` (nuovo) — estende
   `CREATE OR ALTER VIEW Foods_Dashboard` con subquery `STRING_AGG` su `FoodCategory`/`Categories`
   → `CategoriesText`. Eseguita subito su DB dev con sqlcmd dopo la creazione.
2. `src\Dr.NutrizioNino.Api\Infrastructure\Models\FoodDashboardInfo.cs` — aggiunge
   `public string? CategoriesText { get; set; }`.
3. `src\Dr.NutrizioNino.WebVue\src\Interfaces\foods\FoodDashboardDto.ts` — aggiunge
   `categoriesText: string | null`.
4. `src\Dr.NutrizioNino.WebVue\src\components\Foods\FoodsList.vue` — nuova colonna dopo "Marca",
   stesso render pattern di `supermarketsText` (chip `NTag`, `type: 'warning'` per distinguerla).

## Fuori scope

- `FoodDetail.vue` / form editing (categoria già gestita lì)
- `FoodDto.cs` backend "pieno" (solo create/edit, non dashboard)
- `CategoryQuickAddModal.vue`, `Category.cs`, `FoodCategoryConfiguration.cs` (invariati)
- Nessun filtro/ricerca per categoria in griglia (non richiesto)

## Criteri di verifica

- [x] Migration SQL eseguita su DB dev (sqlcmd); `SELECT TOP 5 Name, CategoriesText FROM Foods_Dashboard`
      mostra `CategoriesText` valorizzato per alimenti con categorie assegnate
- [x] Backend compila (`dotnet build`) — 0 errori
- [x] Frontend: `vue-tsc --noEmit` pulito — 0 errori
- [ ] Frontend: verifica visiva manuale in browser (dev server) — da fare live dall'utente
- [x] Colonne esistenti (Marca, Supermercati, Azioni) invariate (solo inserita nuova colonna tra Marca e UdM)
