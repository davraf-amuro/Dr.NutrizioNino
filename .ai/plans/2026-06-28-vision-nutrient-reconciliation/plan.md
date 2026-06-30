# Piano — Riconciliazione nutrienti OCR (Vision) + refactor FE Vue

Stato: COMPLETATO
Data: 2026-06-28
Modello: claude-opus-4-8

## Obiettivo

Completare il flusso "maschera nuovo alimento con OCR via Ollama" secondo i requisiti chiariti dall'utente:

- L'utente manda l'etichetta a Ollama, riceve JSON, **vede e capisce** il risultato a video.
- L'utente è responsabile della correttezza del dato al **Salva** (unico gesto definitivo sull'alimento).
- I valori JSON vanno associati correttamente ai campi FE.
- Quantità errate → l'utente corregge inline.
- UoM selezionata errata → cerca tra quelle disponibili; se non trovata → proponi inserimento a DB **subito**, il Salva viene dopo.
- Nutriente inesistente → l'utente lo inserisce a DB **subito**, il Salva viene dopo.
- Le anagrafiche di supporto (UoM, nutriente) si persistono a DB **prima** del Salva alimento, non contestualmente → al Salva le FK esistono già.

Output warroom (opus): convergenza su tutti i gap. Pattern: rimuovere ridondanza `UnitaMisura` (viola 3NF — dipendenza transitiva via `DefaultUnitOfMeasureId`), single source of truth = FK.

## Scope

### IN scope
- Backend: validator UoM obbligatoria su create nutriente; refactor rimozione `UnitaMisura`; gestione idempotente duplicati.
- Frontend Vue: indicatore stato match inline; modal nutriente con selettore UoM; warning specifico; flusso creazione UoM mancante; propagazione stato centralizzata.
- 1 migration SQL (DROP colonna ridondante) + cleanup indice ridondante.

### Perimetro negativo (NON toccare)
- Dishes, DailySimulation, Auth, Categories, Brands, Supermarkets.
- Logica provider Ollama / VisionProviderFactory / cache estrazione.
- Dedup kJ/kcal (già corretto).

## Fatti verificati

- DB: `Nutrients` ha sia `DefaultUnitOfMeasureId` (FK NOT NULL) sia `UnitaMisura` nvarchar(20) NOT NULL → **ridondanza** (copia di `UnitsOfMeasures.Abbreviation`, popolata in `NutrientService.cs:38`).
- DB: `NutrientAlias.AiName` ha già `UQ_NutrientAlias_AiName` (UNIQUE) → no migration indice unico. Esiste `IX_NutrientAlias_AiName` non-unique **ridondante** → candidato DROP.
- FE: nessun uso di `unitaMisura` (zero match) → DROP senza impatto sui read FE.
- `CreateNutrientDto` (BE) e `CreateNutrientRequest` (FE) hanno già `DefaultUnitOfMeasureId` opzionale.
- `Nutrient.cs` NON ha navigation property verso `UnitOfMeasure`.
- `POST /nutrients` esiste, ritorna 409 su nome duplicato.

## Fasi

### FASE 1 — Backend: validazione UoM obbligatoria su create nutriente
- [ ] Creare `Validators/CreateNutrientValidator.cs` (`IValidator<CreateNutrientDto>`): `DefaultUnitOfMeasureId` NotEmpty + check esistenza FK async su `UnitsOfMeasures`.
- [ ] Agganciare il validator nell'handler `POST /nutrients` prima della logica → 400 su fallimento; aggiungere `Produces(400)`.
- Criterio: POST senza UoM o con UoM inesistente → 400 ProblemDetails.

### FASE 2 — Backend: refactor rimozione `UnitaMisura` (expand)
- [ ] Aggiungere navigation property `Nutrient.DefaultUnitOfMeasure` (+ config EF in `NutrientConfiguration.cs`).
- [ ] `VisionExtractionService`: sostituire le proiezioni `n.UnitaMisura` con `n.DefaultUnitOfMeasure.Abbreviation` (righe 21, 23, 121, 151, 159-160). Mantenere matching case-insensitive.
- [ ] `ModelsMapper.cs`, `NutrientExtensions.cs`: proiettare `Abbreviation` via nav property (EF-traducibile, member access).
- [ ] `NutrientService.cs`: rimuovere la sync manuale `UnitaMisura` (righe 35-46).
- Criterio: build OK, estrazione nutrienti funziona leggendo l'abbreviazione via join.

### FASE 3 — Backend: DTO + migration (contract)
- [ ] Rimuovere `UnitaMisura` da `NutrientiDto.cs` e `NutrientInfo.cs` (+ aggiornare costruzioni).
- [ ] Rimuovere `public string UnitaMisura` da `Nutrient.cs`.
- [ ] Migration SQL `schema-migrations/2026-06-28_drop-nutrient-unitamisura.sql`: `DROP COLUMN UnitaMisura` + `DROP INDEX IX_NutrientAlias_AiName` (ridondante). Eseguire via sqlcmd.
- Criterio: build OK, DB senza colonna ridondante, nessun riferimento residuo a `UnitaMisura`.

### FASE 4 — Backend: idempotenza create nutriente/alias
- [ ] Verificare endpoint alias (`POST /nutrients/aliases`): su violazione UNIQUE (2627/2601) restituire entità esistente invece di 500.
- [ ] `POST /nutrients`: confermare gestione 409 nome duplicato già adeguata per il FE.
- Criterio: doppio POST concorrente non genera 500.

### FASE 5 — FE: indicatore stato match inline (`FoodNutrientInput.vue`)
- [ ] Prop opzionale `status?: ExtractionStatus`.
- [ ] Badge accessibile a sinistra della label: icona + testo + `aria-label` (token Naive UI success/warning/error). No colore-solo (WCAG 1.4.1).
- Criterio: riga mostra verde/giallo/rosso con etichetta testuale leggibile da screen reader.

### FASE 6 — FE: modal nutriente con selettore UoM (`NutrientQuickAddModal.vue`)
- [ ] Allineare a `UnitQuickAddModal`: aggiungere `NSelect` UoM **obbligatorio**, alimentato da prop `unitsOfMeasures` passata da `FoodDetail` (no fetch duplicato).
- [ ] Accettare prop `suggestedName` e `suggestedUnit` (pre-compila nome + tenta preselezione UoM per abbreviation).
- [ ] Emettere `created` con `{ nutrient, unitOfMeasureId }`.
- Criterio: nuovo nutriente creato sempre con UoM valida; payload contiene `defaultUnitOfMeasureId`.

### FASE 7 — FE: warning specifico + flusso UoM mancante (`ExtractionReconciliation.vue`)
- [ ] Warning IncompleteMatch come stringa computata: `atteso: <abbreviation>, trovato: <unit OCR>`.
- [ ] Quando UoM OCR non trovata tra le disponibili → azione "Crea unità" che apre `UnitQuickAddModal` (pre-compila abbreviation), salva subito a DB, poi riseleziona sulla riga.
- Criterio: l'utente vede la discrepanza esatta e può creare la UoM senza uscire dal flusso.

### FASE 8 — FE: orchestrazione stato centralizzato (`FoodDetail.vue`)
- [ ] Mappa stato match per `nutrientId` derivata da `lastExtractionResults`, propagata a `FoodNutrientInput` (status) e a `ExtractionReconciliation`.
- [ ] Passare `unitsOfMeasures` a `NutrientQuickAddModal`.
- [ ] Gestire `created {nutrient, unitOfMeasureId}`: push nutriente in `localFood.nutrients` con la UoM scelta (non default).
- [ ] Le anagrafiche si salvano subito a DB; il dato alimento resta bozza fino a Salva.
- Criterio: un solo modello di stato, righe + pannello sincronizzati; al Salva tutte le FK esistono.

### FASE 9 — Verifica finale
- [ ] `dotnet format` sui csproj toccati (gate push).
- [ ] `npm run lint` su WebVue (se script presente).
- [ ] Test manuale flusso: incolla etichetta → 6 match (verde) / 1 warning / 1 error → crea UoM mancante → crea nutriente con UoM → Salva.

## Criteri di verifica (tutti obbligatori a fine task)
- [ ] Nessun riferimento residuo a `UnitaMisura` nel codice (grep pulito).
- [ ] `POST /nutrients` rifiuta UoM mancante/inesistente con 400.
- [ ] Migration eseguita su DB; colonna e indice ridondante rimossi.
- [ ] Righe nutriente mostrano stato match accessibile.
- [ ] Nuovo nutriente creato con UoM scelta dall'utente, propagata alla riga.
- [ ] UoM mancante creabile inline senza uscire dal flusso.
- [ ] Build backend + FE OK, lint clean.
