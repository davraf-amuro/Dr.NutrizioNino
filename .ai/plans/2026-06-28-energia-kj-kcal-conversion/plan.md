# Piano — Energia: leggere kcal stampato + fallback conversione

Stato: COMPLETATO (superato da approccio OCR-pairing + dedupe, vedi piano hedgehog)
Data: 2026-06-28
Modello: claude-opus-4-8

## Obiettivo

Il modello restituisce Energia in kJ (2252) invece del kcal stampato (539).
Vincolo utente: se l'etichetta riporta kcal, va usato il valore STAMPATO (539),
non una conversione (2252/4.184 = 538.3 != 539). Fix primario nel prompt; la
conversione DB kJ->kcal serve solo come fallback per etichette con solo kJ.

## Scope

- `src/Dr.NutrizioNino.Api/Services/VisionExtractionService.cs` — sezione ENERGIA del prompt.
- Nuovo `schema-migrations/2026-06-28_energia-kj-kcal-conversion.sql` + esecuzione sqlcmd.

Perimetro negativo: NON tocco mapper, body Ollama, altri provider, DTO, altre conversioni.

## Fasi

- [ ] Fase 1 — Riscrivere la regola ENERGIA nel prompt: leggi il numero accanto a "kcal"
      (es. 539), mai quello accanto a "kJ"; converti solo se kcal assente.
- [ ] Fase 2 — Creare lo script SQL (2 INSERT idempotenti kJ<->kcal).
- [ ] Fase 3 — Eseguire lo script via sqlcmd.
- [ ] Fase 4 — dotnet build Api.

## Criteri di verifica

- [ ] Prompt: ramo "leggi kcal stampato" prima di "converti".
- [ ] Re-test foto: Energia = 539 kcal (stampato).
- [ ] UnitConversions contiene (kJ,kcal) e (kcal,kJ).
- [ ] Build Api 0 errori.
