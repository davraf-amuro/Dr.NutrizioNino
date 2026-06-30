# Piano: Vision UI Fixes + Validator

**Stato:** IN CORSO
**Data:** 2026-06-21
**Slug:** vision-ui-fixes

## Obiettivo

Correggere 4 problemi del componente Vision Extraction + aggiungere `IValidator<T>` sull'endpoint `extract-nutrients`.

## Scope

### File da modificare
- `src/Dr.NutrizioNino.Api/Services/VisionExtractionService.cs`
- `src/Dr.NutrizioNino.Api/Endpoints/FoodVisionMapping.cs`
- `src/Dr.NutrizioNino.WebVue/src/components/Foods/FoodDetail.vue`
- `src/Dr.NutrizioNino.Api/Program.cs`

### File da creare
- `src/Dr.NutrizioNino.Api/Validators/IValidator.cs` (base interface — non esiste ancora)
- `src/Dr.NutrizioNino.Api/Validators/ExtractNutrientsRequestValidator.cs`
- `Dr.NutrizioNino.Models/Dto/ExtractNutrientsRequest.cs` (sposta da private record in Mapping a DTO pubblico)

### Fuori scope
- `UnitConversionService.cs` — non toccare la logica di conversione backend
- Altri endpoint/componenti non citati

## Fasi

### Fase 1 — BE: Fix confronto unità case-insensitive
- [ ] `VisionExtractionService.cs:138`
- Cambia: `matched.UnitaMisura == r.Unit`
- In: `string.Equals(matched.UnitaMisura, r.Unit, StringComparison.OrdinalIgnoreCase)`

### Fase 2 — FE: Fix "Apri immagine" — window.open con data URL bloccato
- [ ] `FoodDetail.vue:136-138` — sostituisci `<img>` con `<n-image>` (senza `preview-disabled`)
- [ ] `FoodDetail.vue:112-120` — rimuovi pulsante "Apri immagine"
- [ ] `FoodDetail.vue:355-357` — rimuovi funzione `openImageInNewWindow`
- Motivo: `window.open(dataUrl, '_blank')` bloccato da Chrome 92+ / Firefox per policy sicurezza sui data URL. Soluzione: lightbox nativo `<n-image>` Naive UI.

### Fase 3 — FE: Fix unità estratta (usa value+unit originali, non convertedValue)
- [ ] `FoodDetail.vue:376-391` — logica `handleImageExtraction`
- Cambia: `extracted.convertedValue ?? extracted.value` → `extracted.value`
- Cambia: lookup UoM via `extracted.canonicalUnit` → via `extracted.unit`
- Rimuovi: push in `extractionConversions` (la conversione non viene più applicata)

### Fase 4 — FE: Pulsante "Carica" da file system
- [ ] `FoodDetail.vue` — aggiungi `<input type="file" accept="image/*">` nascosto
- [ ] `FoodDetail.vue` — aggiungi pulsante "📂 Carica" dopo "Incolla"
- [ ] `FoodDetail.vue` — aggiungi ref `fileInput` + metodo `onFileSelected`
- [ ] Estrai funzione condivisa `loadImageFromFile(file: File)` riusando logica FileReader da `triggerImagePaste:430-437`

### Fase 5 — FE: Label "Incolla" + aria-label
- [ ] `FoodDetail.vue:99-101`
- Cambia: `"📷 Incolla immagine etichetta"` → `"📋 Incolla"`
- Aggiungi: `aria-label="Incolla immagine etichetta dagli appunti"`

### Fase 6 — BE Bonus: IValidator<T> su ExtractNutrientsRequest
- [ ] Sposta `ExtractNutrientsRequest` da private record in `FoodVisionMapping.cs` a `Dr.NutrizioNino.Models/Dto/ExtractNutrientsRequest.cs` (public record)
- [ ] Crea `src/Dr.NutrizioNino.Api/Validators/IValidator.cs` (base interface + ValidationResult)
- [ ] Crea `src/Dr.NutrizioNino.Api/Validators/ExtractNutrientsRequestValidator.cs`
  - `Base64Image`: required, non empty
  - `ProviderKey`: required, non empty
  - `MediaType`: default `"image/jpeg"` già nel record — nessuna regola aggiuntiva
- [ ] `Program.cs` — registra `AddScoped<IValidator<ExtractNutrientsRequest>, ExtractNutrientsRequestValidator>()`
- [ ] `FoodVisionMapping.cs` — inietta validator, rimuovi guard manuali, usa `TypedResults.ValidationProblem`

## Criteri di verifica

- [ ] Compilazione .NET senza errori (`dotnet build`)
- [ ] "Sale 15 mg" importato come `quantity=15, unit=mg` senza conversione visibile
- [ ] Click su anteprima immagine apre lightbox Naive UI (nessuna finestra vuota)
- [ ] Pulsante "Carica" apre dialog file e avvia estrazione
- [ ] Pulsante si chiama "📋 Incolla" con aria-label
- [ ] POST `extract-nutrients` con body vuoto → 400 ValidationProblem (non 400 BadRequest manuale)
- [ ] Lint .NET: `dotnet format --verify-no-changes`
