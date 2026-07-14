# Piano: Suggerimento alimenti con nome simile in fase di inserimento

Data: 2026-07-14
Stato: COMPLETATO

Verificato: build+tsc puliti, e testato manualmente live su `/foods` dall'utente — funziona.

## Obiettivo

Mentre l'utente digita il nome di un nuovo alimento in `/foods`, mostrare (non bloccante) un elenco
di alimenti esistenti con nome simile, per aiutarlo a scegliere il nome più appropriato ed evitare
doppioni ortografici. Esito warroom (5/5, vedi sintesi in conversazione): endpoint dedicato leggero
(non riuso del filtro dashboard esistente), suggerimento mai auto-agganciante, azione esplicita di
conferma ("Usa questo esistente"), nessun intervento DB (23 righe, LIKE senza indice è già ottimale).

## Fatti verificati (no agenti aggiuntivi — esplorazione diretta già sufficiente)

- BE: `FoodEndpoints.cs` già ha `IsFoodNameTakenAsync` (duplicato ESATTO, usato in Create/Update per 409)
  e `GetFoodsDashboardAsync(nameFilter)` (partial match, ma ritorna `FoodDashboardInfo` completo — troppo
  pesante per un typeahead, non riusabile secondo warroom ARCH/BE).
- BE: `DrRepository.Food.cs:87-93` ha già il pattern esatto da copiare: `EF.Functions.Like(f.Name!, $"%{nameFilter}%")`.
- BE: `Program.cs:148-167` ha rate limiter con policy `"vision"` (3/min) e `"aliases"` (10/min) — serve
  una nuova policy più permissiva per digitazione live.
- FE: `FoodDetail.vue:7-9` — campo nome è `n-input` con `v-model:value="localFood.name"`, `path="name"`
  nel form validation. Scelgo di NON sostituirlo con `n-auto-complete` (rischio di rompere il binding
  di validazione `rules`/`path`) — aggiungo invece una lista di suggerimenti sotto il campo, mostrata
  solo in creazione (non in modifica) quando arrivano risultati.
- FE: `FoodsView.vue` ha già `startEditFood(id)` per passare dalla lista al form di modifica — riuso
  per l'azione "Usa questo esistente" (annulla la creazione, apre l'alimento esistente in modifica).

## Scope

### File da modificare/creare
- [x] `Dr.NutrizioNino.Models/Dto/FoodSuggestionDto.cs` (nuovo) — `record FoodSuggestionDto(Guid Id, string Name)`
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository.Food.cs` — nuovo metodo `GetSimilarFoodNamesAsync(query, take, ct)`, stesso pattern di `GetFoodsDashboardAsync` ma proiezione minima + ordinamento prefisso-prima
- [x] `src/Dr.NutrizioNino.Api/Services/FoodService.cs` — nuovo metodo `FindSimilarNamesAsync(query, ct)`, guard lunghezza minima 2 caratteri
- [x] `src/Dr.NutrizioNino.Api/Endpoints/FoodEndpoints.cs` — nuovo `GET similar?query=`, rate limiting dedicato, TOP 8
- [x] `src/Dr.NutrizioNino.Api/Program.cs` — nuova policy rate limiter `"food-search"` (30/min, sliding window)
- [x] `src/Dr.NutrizioNino.WebVue/src/Interfaces/foods/FoodSuggestionDto.ts` (nuovo)
- [x] `src/Dr.NutrizioNino.WebVue/src/modules/foods/api/foods.api.ts` — nuova funzione `getSimilarFoodNames(query, signal)`
- [x] `src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue` — watcher debounced su nome, lista suggerimenti sotto il campo (solo in creazione), bottone "Usa questo esistente" → emit `use-existing`
- [x] `src/Dr.NutrizioNino.WebVue/src/views/FoodsView.vue` — handler `@use-existing` → `startEditFood(id)` + `cancelCreateFood()`

### Perimetro negativo
- Non tocco: `GetFoodsDashboardAsync`/dashboard esistente, `IsFoodNameTakenAsync` (duplicato esatto, resta invariato e continua a bloccare al submit).
- Non aggiungo indici o full-text search a DB (DBADMIN: non giustificato a 23 righe).
- Non implemento "Crea nuovo comunque" come bottone esplicito: ignorare il suggerimento e continuare/confermare è già il comportamento di default (nessuna azione bloccante).
- Non sostituisco `n-input` con `n-auto-complete` (rischio regressione sul binding di validazione form).

## Fasi

### Fase 1: DTO
- **Stato**: [x]
- **File**: `Dr.NutrizioNino.Models/Dto/FoodSuggestionDto.cs`
- **Operazione**: CREATE
- **Azione**: `public record FoodSuggestionDto(Guid Id, string Name);`
- **Verifica passo**: file creato, namespace corretto

### Fase 2: Repository
- **Stato**: [x]
- **Precondizione**: Fase 1 completata
- **File**: `src/Dr.NutrizioNino.Api/Infrastructure/DrRepository.Food.cs`
- **Operazione**: EDIT
- **Azione**: aggiungere `GetSimilarFoodNamesAsync(string query, int take, CancellationToken ct)` — proiezione `new FoodSuggestionDto(f.Id, f.Name)`, `Where(EF.Functions.Like(f.Name, $"%{query}%"))`, `OrderByDescending(f.Name.ToLower().StartsWith(query.ToLower())).ThenBy(f.Name)`, `Take(take)`
- **Verifica passo**: rilettura file, metodo presente e sintatticamente corretto

### Fase 3: Service
- **Stato**: [x]
- **Precondizione**: Fase 2 completata
- **File**: `src/Dr.NutrizioNino.Api/Services/FoodService.cs`
- **Operazione**: EDIT
- **Azione**: aggiungere `FindSimilarNamesAsync(string query, CancellationToken ct)` — se `query` null/whitespace/lunghezza&lt;2 ritorna lista vuota senza query DB, altrimenti chiama repository con `take=8`
- **Verifica passo**: rilettura file

### Fase 4: Endpoint + rate limiter
- **Stato**: [x]
- **Precondizione**: Fase 3 completata
- **File**: `src/Dr.NutrizioNino.Api/Endpoints/FoodEndpoints.cs`, `src/Dr.NutrizioNino.Api/Program.cs`
- **Operazione**: EDIT
- **Azione**: `GET similar` con query param `query`, chiama `service.FindSimilarNamesAsync`, `Produces<IList<FoodSuggestionDto>>`, `.RequireRateLimiting("food-search")`; in Program.cs aggiungere policy `"food-search"` (PermitLimit 30, Window 1 minuto)
- **Verifica passo**: rilettura file, `dotnet build` 0 errori

### Fase 5: FE — tipo + api client
- **Stato**: [x]
- **Precondizione**: Fase 4 completata
- **File**: `Interfaces/foods/FoodSuggestionDto.ts` (nuovo), `foods.api.ts`
- **Operazione**: CREATE + EDIT
- **Azione**: interfaccia `{ id: string; name: string }`; funzione `getSimilarFoodNames(query, signal)` → `GET /foods/similar?query=`
- **Verifica passo**: `vue-tsc --noEmit` 0 errori

### Fase 6: FE — UI suggerimenti in FoodDetail.vue
- **Stato**: [x]
- **Precondizione**: Fase 5 completata
- **File**: `FoodDetail.vue`
- **Operazione**: EDIT
- **Azione**: watcher debounced (300ms) su `localFood.name` (solo se `!isEditMode`), chiama `getSimilarFoodNames`, mostra lista sotto il campo nome (nome + bottone piccolo "Usa questo esistente"), gestione richieste stale con AbortController; click emette `use-existing` con l'id
- **Verifica passo**: rilettura file, `vue-tsc --noEmit` 0 errori

### Fase 7: FE — gestione evento in FoodsView.vue
- **Stato**: [x]
- **Precondizione**: Fase 6 completata
- **File**: `FoodsView.vue`
- **Operazione**: EDIT
- **Azione**: handler `@use-existing="(id) => { cancelCreateFood(); startEditFood(id) }"`
- **Verifica passo**: rilettura file, `vue-tsc --noEmit` 0 errori

### Fase 8: Verifica finale
- **Stato**: [x]
- **Precondizione**: Fasi 1-7 completate
- **Azione**: `dotnet build` API + `vue-tsc --noEmit` FE
- **Verifica passo**: 0 errori su entrambe

## Criteri di verifica finale
- [x] `GET /foods/similar?query=...` ritorna TOP 8 alimenti con nome simile, case-insensitive, prefisso prima
- [x] Query &lt;2 caratteri → lista vuota, nessuna chiamata DB
- [x] Rate limiting dedicato attivo (429 oltre soglia)
- [x] FE mostra suggerimenti non bloccanti solo in creazione, mai in modifica
- [x] Click "Usa questo esistente" annulla la creazione e apre l'alimento esistente in modifica
- [x] Ignorare i suggerimenti non blocca la creazione (comportamento invariato, `IsFoodNameTakenAsync` continua a proteggere da duplicati esatti al submit)
- [x] Build API 0 errori, `vue-tsc` 0 errori
- [x] Nessun file fuori dal perimetro dichiarato toccato
