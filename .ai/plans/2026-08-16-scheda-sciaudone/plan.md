# Piano: Scheda Sciaudone per utente
Data: 2026-08-16
Stato: IN CORSO

## Obiettivo
Persistere per ogni utente la "scheda Sciaudone" (kcal, proteine, grassi, fibre, carboidrati) calcolata dalle misurazioni di profilo, con storico su tabella dedicata ed esposizione via API + UI.

## Fonte requisito
`TODO/20260816-02.md`

## Decisioni utente (round di chiarimento 2026-08-16)
| Punto | Decisione |
|---|---|
| Carboidrati (punto 5) | Costante fissa: `(kcal − 990) / 4` — il `75 × 13.2` resta letterale, non generalizzato sul peso |
| Peso ideale (punto 2) | Campo inserito dall'utente (`IdealWeightKg`), nessuna formula automatica |
| Persistenza | Tabella dedicata storicizzata `SciaudoneCards`, una riga per misurazione |
| Guardrail di sicurezza | Nessuno — formule applicate alla lettera, nessun floor kcal, nessun clamp |

## Formule (fonte unica: `Helpers/SciaudoneFormula.cs`)
```
Kcal     = WeightKg * 22
ProteinG = IdealWeightKg * 2
FatG     = WeightKg * 0.66
FiberG   = (Kcal / 1000) * 15
CarbsG   = (Kcal - 990) / 4        // 990 = 75 * 13.2, costante fissa per scelta utente
```
Scheda generata solo se `WeightKg` **e** `IdealWeightKg` sono valorizzati; altrimenti nessuna riga.

## Note nutrizionali (Coach — registrate, non applicate per scelta utente)
- La costante 990 chiude il bilancio energetico solo per un peso di 75 kg; per altri pesi kcal ≠ 4P + 9F + 4C.
- 22 kcal/kg su peso attuale può scendere sotto le soglie di sicurezza comunemente citate (1200 kcal F / 1500 kcal M).
- Fibre 15 g/1000 kcal è coerente con IOM 2004 (14 g/1000 kcal) e range 12.6–16.7.
- Il campo `Job` (livello attività) non entra nel calcolo: la scheda è forfait, non un TDEE.
- Copy UI obbligatorio: "stima generica, non sostituisce una consulenza nutrizionale".

## Scope

### File da creare
- [ ] `schema-migrations/2026-08-16_sciaudone-card.sql` — ALTER UserProfileEntries + CREATE SciaudoneCards
- [ ] `src/Dr.NutrizioNino.Api/Infrastructure/Models/SciaudoneCard.cs` — entità EF
- [ ] `src/Dr.NutrizioNino.Api/Infrastructure/Models/Configurations/SciaudoneCardConfiguration.cs` — mapping
- [ ] `src/Dr.NutrizioNino.Api/Infrastructure/DrNutrizioNinoContext.SciaudoneCard.cs` — DbSet partial
- [ ] `src/Dr.NutrizioNino.Api/Helpers/SciaudoneFormula.cs` — costanti + calcolo (fonte unica)
- [ ] `src/Dr.NutrizioNino.Api/Services/SciaudoneCardService.cs` — generazione e lettura schede
- [ ] `src/Dr.NutrizioNino.Api/Validators/AddProfileEntryRequestValidator.cs` — validazione body POST profilo
- [ ] `Dr.NutrizioNino.Models/Dto/Auth/SciaudoneCardDto.cs` — DTO risposta
- [ ] `src/Dr.NutrizioNino.Api/UserProfile.http` — chiamate di prova nuovi endpoint
- [ ] `src/Dr.NutrizioNino.WebVue/src/Interfaces/userProfile/SciaudoneCardDto.ts` — tipo FE
- [ ] `src/Dr.NutrizioNino.WebVue/src/Interfaces/userProfile/ProfileEntryDto.ts` — tipo FE misurazione
- [ ] `src/Dr.NutrizioNino.WebVue/src/modules/userProfile/api/userProfile.api.ts` — client API
- [ ] `src/Dr.NutrizioNino.WebVue/src/components/UserProfile/SciaudoneCardPanel.vue` — visualizzazione scheda

### File da modificare
- [ ] `src/Dr.NutrizioNino.Api/Infrastructure/DrNutrizioNinoContext.cs` — ApplyConfiguration della nuova entità
- [ ] `src/Dr.NutrizioNino.Api/Infrastructure/Models/UserProfileEntry.cs` — campo `IdealWeightKg`
- [ ] `src/Dr.NutrizioNino.Api/Infrastructure/Models/Configurations/UserProfileEntryConfiguration.cs` — mapping `IdealWeightKg`
- [ ] `src/Dr.NutrizioNino.Api/Services/UserProfileService.cs` — genera la scheda dopo il salvataggio della misurazione
- [ ] `src/Dr.NutrizioNino.Api/Endpoints/UserProfileEndpoints.cs` — GET scheda corrente + storico, validazione POST
- [ ] `src/Dr.NutrizioNino.Api/Program.cs` — DI di `SciaudoneCardService` e del validator
- [ ] `Dr.NutrizioNino.Models/Dto/Auth/AddProfileEntryRequest.cs` — campo `IdealWeightKg`
- [ ] `Dr.NutrizioNino.Models/Dto/Auth/ProfileEntryResponse.cs` — campo `IdealWeightKg`
- [ ] `src/Dr.NutrizioNino.WebVue/src/views/UserProfileView.vue` — form misurazione + scheda

### Perimetro negativo
- Non toccherò `NutritionalTargets` (tabella, service, endpoint, modulo FE): il fabbisogno dichiarato manualmente resta indipendente dalla scheda.
- Non toccherò il dominio Recipe/Food/DailySimulation.
- Non toccherò l'autenticazione, i ruoli, il tema utente.
- Non introdurrò guardrail di sicurezza sui valori calcolati (esclusi per decisione utente).
- Nessun `git push` (gate lint umano).

## Fasi

## ✅ Divergenza Fase 2 RISOLTA (2026-08-17)
Il DB è tornato leggibile senza interventi. Migration applicata e verificata: colonna `IdealWeightKg` presente, tabella `SciaudoneCards` con 11 colonne, FK `FK_SciaudoneCards_User` NO_ACTION, FK `FK_SciaudoneCards_ProfileEntry` CASCADE, indice unique su `ProfileEntryId`, indice `IX_SciaudoneCards_UserId_ComputedAt`.

Nota strumenti: MCP `db-schema` è registrato ma ha tutti i flag `Ddl` a `false` in `tools/dr-mcp-dbschema/appsettings.json` — `run_select` e `execute_*` rifiutano. Migration eseguita con sqlcmd, unica via disponibile.

## ⚠️ Divergenza Fase 2 (2026-08-16, storica): database non leggibile
`sqlcmd` fallisce su qualsiasi lettura di `DrNutrizioNino` con `Msg 823 ... OS error 21 (Dispositivo non pronto)` a offset `0x36e000` di `E:\Davide\Database\DrNutrizioNino.mdf`. Il volume `E:` è sano (NTFS, 400 GB liberi, file di progetto leggibili) e `sys.master_files` riporta il DB ONLINE con MDF da 8 MB e LDF da 1 MB. Guasto a livello di file/disco, non SQL. Nessuna azione correttiva intrapresa (no DBCC CHECKDB, no detach, no restore): richiede decisione utente.

Conseguenza: Fase 2 e Fase 9 sospese. Fasi 3–8 (codice) eseguite comunque, verificate con `dotnet build` e `npm run lint`.

### Fase 1: Migration SQL
- **Stato**: [x]
- **Precondizione**: `schema-migrations/` esiste; nessuna tabella `SciaudoneCards` a DB
- **File**: `schema-migrations/2026-08-16_sciaudone-card.sql`
- **Operazione**: CREATE
- **Azione**: scrivere `ALTER TABLE UserProfileEntries ADD IdealWeightKg numeric(5,2) NULL` e `CREATE TABLE SciaudoneCards` (Id PK, UserId FK AspNetUsers NO ACTION, ProfileEntryId FK UserProfileEntries CASCADE + UNIQUE, WeightKg, IdealWeightKg, Kcal, ProteinG, FatG, FiberG, CarbsG, ComputedAt) con blocco `-- DOWN:`
- **Nota**: FK su `UserId` deve essere NO ACTION — con CASCADE si creerebbero due percorsi di cancellazione verso AspNetUsers (errore SQL Server 1785)
- **Tool ammessi**: nessuno
- **Verifica passo**: file presente e rileggibile, contiene entrambi gli statement
- **Su divergenza**: STOP — annota `⚠️ Divergenza Fase 1` in plan.md

### Fase 2: Esecuzione migration su DB
- **Stato**: [x]
- **Precondizione**: Fase 1 completata
- **File**: nessuno (comando)
- **Operazione**: EDIT (database)
- **Azione**: eseguire la migration con `sqlcmd -S localhost -d DrNutrizioNino -E -N -C -I -i <file>`
- **Tool ammessi**: Bash (sqlcmd)
- **Verifica passo**: `SELECT` su `INFORMATION_SCHEMA.COLUMNS` mostra `IdealWeightKg` su UserProfileEntries e le colonne di `SciaudoneCards`
- **Su divergenza**: STOP — annota `⚠️ Divergenza Fase 2` in plan.md

### Fase 3: Entità e mapping EF
- **Stato**: [x]
- **Precondizione**: Fase 2 completata
- **File**: `SciaudoneCard.cs`, `SciaudoneCardConfiguration.cs`, `DrNutrizioNinoContext.SciaudoneCard.cs`, `DrNutrizioNinoContext.cs`, `UserProfileEntry.cs`, `UserProfileEntryConfiguration.cs`
- **Operazione**: CREATE + EDIT
- **Azione**: creare entità e configurazione della scheda, registrare la configuration nel context, aggiungere `IdealWeightKg` a `UserProfileEntry` e al suo mapping
- **Tool ammessi**: nessuno
- **Verifica passo**: `dotnet build` della sola API senza errori
- **Su divergenza**: STOP — annota `⚠️ Divergenza Fase 3` in plan.md

### Fase 4: Formule e DTO
- **Stato**: [x]
- **Precondizione**: Fase 3 completata
- **File**: `Helpers/SciaudoneFormula.cs`, `Dto/Auth/SciaudoneCardDto.cs`, `Dto/Auth/AddProfileEntryRequest.cs`, `Dto/Auth/ProfileEntryResponse.cs`
- **Operazione**: CREATE + EDIT
- **Azione**: costanti nominate (22, 2, 0.66, 15, 990) e metodo di calcolo puro; DTO scheda; aggiunta `IdealWeightKg` a request e response del profilo
- **Tool ammessi**: nessuno
- **Verifica passo**: `dotnet build` senza errori
- **Su divergenza**: STOP — annota `⚠️ Divergenza Fase 4` in plan.md

### Fase 5: Service e validator
- **Stato**: [x]
- **Precondizione**: Fase 4 completata
- **File**: `Services/SciaudoneCardService.cs`, `Services/UserProfileService.cs`, `Validators/AddProfileEntryRequestValidator.cs`
- **Operazione**: CREATE + EDIT
- **Azione**: service che genera e persiste la scheda dalla misurazione e ne espone corrente/storico; `UserProfileService.AddEntryAsync` la invoca dopo il salvataggio; validator sul body POST
- **Tool ammessi**: nessuno
- **Verifica passo**: `dotnet build` senza errori
- **Su divergenza**: STOP — annota `⚠️ Divergenza Fase 5` in plan.md

### Fase 6: Endpoint e DI
- **Stato**: [x]
- **Precondizione**: Fase 5 completata
- **File**: `Endpoints/UserProfileEndpoints.cs`, `Program.cs`, `UserProfile.http`
- **Operazione**: EDIT + CREATE
- **Azione**: `GET .../profile/sciaudone` (corrente, 404 se assente) e `GET .../profile/sciaudone/history`, validazione nel POST con 400, registrazione DI di service e validator, file .http di prova
- **Tool ammessi**: nessuno
- **Verifica passo**: `dotnet build` senza errori; endpoint presenti nel file con metadata OpenAPI completi
- **Su divergenza**: STOP — annota `⚠️ Divergenza Fase 6` in plan.md

### Fase 7: Tipi e client API frontend
- **Stato**: [x]
- **Precondizione**: Fase 6 completata
- **File**: `Interfaces/userProfile/SciaudoneCardDto.ts`, `Interfaces/userProfile/ProfileEntryDto.ts`, `modules/userProfile/api/userProfile.api.ts`
- **Operazione**: CREATE
- **Azione**: tipi allineati ai DTO backend e funzioni `getCurrentProfileEntry`, `addProfileEntry`, `getSciaudoneCard`, `getSciaudoneHistory`
- **Tool ammessi**: nessuno
- **Verifica passo**: `npm run lint` senza errori sui file nuovi
- **Su divergenza**: STOP — annota `⚠️ Divergenza Fase 7` in plan.md

### Fase 8: UI scheda e form misurazione
- **Stato**: [x]
- **Precondizione**: Fase 7 completata
- **File**: `components/UserProfile/SciaudoneCardPanel.vue`, `views/UserProfileView.vue`
- **Operazione**: CREATE + EDIT
- **Azione**: pannello che mostra i cinque valori della scheda con disclaimer; form di inserimento misurazione (peso, peso ideale, altezza, sesso, attività) che ricarica la scheda dopo il salvataggio
- **Tool ammessi**: nessuno
- **Verifica passo**: `npm run lint` e `npm run build` senza errori
- **Su divergenza**: STOP — annota `⚠️ Divergenza Fase 8` in plan.md

### Fase 9: Verifica end-to-end
- **Stato**: [~] PARZIALE — mancano solo le prove autenticate
- **Fatto**: `dotnet build` 0 errori · `npm run lint` pulito · migration applicata e struttura verificata · modello EF caricato e DB inizializzato all'avvio dell'API · rotte `sciaudone` e `sciaudone/history` rispondono 401 (mappate) contro 404 di una rotta inesistente
- **Da fare**: POST di una misurazione con token valido → riga in `SciaudoneCards`; POST senza peso ideale → nessuna scheda; POST con body non valido → 400. `SciaudoneCards` e `UserProfileEntries` sono entrambe a 0 righe: nessun dato di prova disponibile e nessuna credenziale per generarlo.
- **Precondizione**: Fase 8 completata
- **File**: nessuno
- **Operazione**: verifica
- **Azione**: `dotnet build` sulla soluzione, `npm run lint` sul frontend, controllo a DB della riga scheda generata da una misurazione di prova
- **Tool ammessi**: Bash, sqlcmd
- **Verifica passo**: build e lint puliti; riga presente in `SciaudoneCards` coerente con le formule
- **Su divergenza**: STOP — annota `⚠️ Divergenza Fase 9` in plan.md

## Criteri di verifica finale
- [ ] Tabella `SciaudoneCards` esiste a DB con FK verso `UserProfileEntries` e `AspNetUsers`
- [ ] Colonna `IdealWeightKg` presente su `UserProfileEntries`
- [ ] Una nuova misurazione con peso e peso ideale genera e persiste una riga scheda
- [ ] Misurazione senza peso o senza peso ideale non genera scheda e non solleva errori
- [ ] `GET /api/v1/users/me/profile/sciaudone` restituisce la scheda corrente, 404 se assente
- [ ] `GET /api/v1/users/me/profile/sciaudone/history` restituisce lo storico ordinato per data discendente
- [ ] POST misurazione con body non valido restituisce 400 con dettaglio campi
- [ ] Le cinque costanti delle formule vivono in un solo file, nessun valore letterale ripetuto
- [ ] La UI mostra i cinque valori e il disclaimer "stima generica"
- [ ] `dotnet build` e `npm run lint` puliti
