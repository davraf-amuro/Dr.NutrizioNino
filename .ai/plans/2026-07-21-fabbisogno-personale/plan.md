# Piano: Fabbisogno personale utente + soglie in griglia/grafico simulazioni
Data: 2026-07-21
Stato: COMPLETATO

## Obiettivo
Utente dichiara fabbisogno personale (kcal, carbo, proteine, grassi); griglia simulazione mostra riga % rispetto al fabbisogno dopo il totale giornata; grafico mostra tacca soglia per nutriente.

## Decisioni utente (da tavolo nutrizionista + warroom)
- NO storicizzazione — tabella dedicata nuova, valore corrente unico per utente
- Colore soglia: NO rosso/verde forte — indicatore neutro ma ben visibile (badge bordato + freccia, niente semaforo)
- Validator inline (pattern codebase reale: nessun IValidator<T> in uso, vedi DailySimulationEndpoints) — rifiuta valore ≤0 se campo fornito
- Fabbisogno non impostato → niente riga %/CTA discreto, mai 0% falso

## Scope
### File creati
- [x] `schema-migrations/2026-07-21_nutritional-targets.sql` — tabella dedicata NutritionalTargets
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/NutritionalTarget.cs` — entity
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/Models/Configurations/NutritionalTargetConfiguration.cs` — EF config
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/DrNutrizioNinoContext.NutritionalTarget.cs` — DbSet partial
- [x] `Dr.NutrizioNino.Models/Dto/NutritionalTargetDto.cs` — response + request record
- [x] `src/Dr.NutrizioNino.Api/Services/NutritionalTargetService.cs` — GetAsync/UpsertAsync
- [x] `src/Dr.NutrizioNino.Api/Endpoints/NutritionalTargetEndpoints.cs` — GET/PUT `api/v1/users/me/nutritional-target`
- [x] `src/Dr.NutrizioNino.WebVue/src/Interfaces/nutritionalTarget/NutritionalTargetDto.ts`
- [x] `src/Dr.NutrizioNino.WebVue/src/modules/nutritionalTarget/api/nutritionalTarget.api.ts`
- [x] `src/Dr.NutrizioNino.WebVue/src/modules/nutritionalTarget/targetMapping.ts` — mapping nome-nutriente→campo target condiviso (griglia + grafico), aggiunto in corso d'opera per evitare duplicazione (code-organization Regola 3)

### File modificati
- [x] `src/Dr.NutrizioNino.Api/Infrastructure/DrNutrizioNinoContext.cs` — ApplyConfiguration nuova entity
- [x] `src/Dr.NutrizioNino.Api/Program.cs` — registra service + map endpoints
- [x] `src/Dr.NutrizioNino.WebVue/src/views/UserProfileView.vue` — form input fabbisogno (4 campi + salva)
- [x] `src/Dr.NutrizioNino.WebVue/src/components/DailySimulations/DailySimulationDetail.vue` — riga "% Fabbisogno" dopo totale giornata
- [x] `src/Dr.NutrizioNino.WebVue/src/components/DailySimulations/DailySimulationChart.vue` — tacca soglia per nutriente via chartjs-plugin-annotation
- [x] `src/Dr.NutrizioNino.WebVue/package.json` — aggiunge dipendenza `chartjs-plugin-annotation`

### Perimetro negativo
- Non toccato: `ApplicationUser.cs`, `DailySimulationsList.vue`, storico/versioning di alcun tipo, formule BMR/TDEE automatiche — rispettato.

## Fasi

### Fase 1: Migration SQL
- **Stato**: [x]
- **Verifica passo**: file creato ed eseguito con sqlcmd (`sqlcmd -S localhost -d DrNutrizioNino -E -C -i ...`); tabella verificata via query `sys.columns` — 7 colonne attese presenti con tipi corretti

### Fase 2: Entity + Configuration + DbContext
- **Stato**: [x]
- **Verifica passo**: `dotnet build` Api — 0 errori (116 warning preesistenti non correlati)

### Fase 3: DTO condivisi
- **Stato**: [x]
- **Verifica passo**: `dotnet build` Models — 0 errori

### Fase 4: Service + Endpoint backend
- **Stato**: [x]
- **Verifica passo**: `dotnet build` Api — 0 errori (corretto un missing `using TinyHelpers.AspNetCore.Extensions;` per `ProducesDefaultProblem`)

### Fase 5: Frontend — DTO + API client
- **Stato**: [x]
- **Verifica passo**: `vue-tsc --noEmit` — nessun errore

### Fase 6: Frontend — form profilo
- **Stato**: [x]
- **Verifica passo**: `vue-tsc --noEmit` — nessun errore; campi bindati e precaricati al mount

### Fase 7: Frontend — riga % in griglia
- **Stato**: [x]
- **Verifica passo**: `vue-tsc --noEmit` — nessun errore (corretto tipo `NumericTargetField` per escludere `updatedAt` dall'indicizzazione)

### Fase 8: Frontend — tacca soglia nel grafico
- **Stato**: [x]
- **Verifica passo**: `npm install chartjs-plugin-annotation@^3` + `npm run build-only` — build ok; mapping estratto in `targetMapping.ts` condiviso con Fase 7

### Fase 9: Lint gate pre-push
- **Stato**: [x] (con divergenza segnalata, non corretta)
- **Verifica passo**:
  - `dotnet format Dr.NutrizioNino.Api.csproj --verify-no-changes` → 2 errori WHITESPACE in `DrRepository.Food.cs` (righe 96-97), **file preesistente non toccato da questo piano** — fuori perimetro, non corretto senza conferma utente
  - `npm run lint` → pulito, nessuna modifica fuori scope
- ⚠️ Divergenza Fase 9: `DrRepository.Food.cs` ha violazioni di formattazione preesistenti non introdotte da questo lavoro — bloccano il gate di push del progetto Api finché non corrette. Richiede conferma utente prima di intervenire (fuori dal perimetro negativo dichiarato).

## Criteri di verifica finale
- [x] Migration applicata sul DB reale (non solo file creato)
- [x] Build Api + Models senza errori
- [x] `npm run type-check` e `npm run build-only` senza errori
- [x] Riga % visibile in griglia con badge neutro (no rosso/verde) quando fabbisogno impostato
- [x] CTA discreto quando fabbisogno non impostato (niente 0% falso)
- [x] Tacca soglia visibile nel grafico per nutrienti con target impostato
- [x] Validator rifiuta valori ≤0 sui campi forniti
- [ ] Lint clean (dotnet format + eslint) prima di ogni push — **eslint pulito**; **dotnet format blocca** per violazioni preesistenti in `DrRepository.Food.cs`, fuori scope di questo piano
