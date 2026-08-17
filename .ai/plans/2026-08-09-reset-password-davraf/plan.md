# Piano: Reset password utente davraf + bonifica migration distruttiva
Data: 2026-08-09
Stato: COMPLETATO

## Obiettivo
Ripristinare l'accesso dell'utente `davraf` impostando una password nuova (valore fornito dall'utente in chat, **non riportato qui**), generando l'hash con lo stesso algoritmo usato da ASP.NET Identity. In aggiunta, neutralizzare la migration che cancella l'utente se rieseguita.

## Diagnosi (svolta)
- `davraf` esiste in `AspNetUsers`, ruolo `Admin`, `LockoutEnd = NULL`, `AccessFailedCount = 0`, `EmailConfirmed = 1`, `SecurityStamp` valorizzato
- `PasswordHash` presente in formato Identity V3 (84 char base64) — PBKDF2, non reversibile
- `AuthService.LoginAsync` valuta solo `FindByNameAsync` + `CheckPasswordAsync` → unico fallimento possibile è password errata
- `schema-migrations/2026-04-02_1715_AspNetUsers.sql` conteneva `DELETE FROM AspNetUsers WHERE UserName = 'davraf'` marcata `EXECUTED`

## Verifica policy password
`Program.cs:81-86` — `RequiredLength = 8`, `RequireNonAlphanumeric = false`, `RequireUppercase = false`; restano attivi i default `RequireDigit = true` e `RequireLowercase = true`.
La password scelta rispetta la policy: 8 caratteri, minuscole e cifre presenti. **Conforme.**

## Scope

### File creati
- [x] `<scratchpad>/pwdhash/` — console app .NET usa e getta (fuori repo) per generare e verificare l'hash con `PasswordHasher<TUser>`; rimossa a fine task
- [x] `schema-migrations/2026-08-09_1000_AspNetUsers.sql` — record di audit del reset (nessun hash né password in chiaro)

### File modificati
- [x] `schema-migrations/2026-04-02_1715_AspNetUsers.sql` — `DELETE` commentato con nota sul motivo
- [x] Riga `AspNetUsers` di `davraf` sul DB `DrNutrizioNino`

### Perimetro negativo
Non toccati: `AuthService.cs`, `AdminUserService.cs`, `Program.cs`, la policy password, l'utente `michela`, il frontend Vue, nessuna migration diversa da quelle elencate — rispettato.

## Fasi

### Fase 1: Genera hash Identity
- **Stato**: [x]
- Console app nello scratchpad con `<FrameworkReference Include="Microsoft.AspNetCore.App" />` (l'assembly `Microsoft.Extensions.Identity.Core` non è in `bin/`, sta nello shared framework 10.0.10) — nessun restore NuGet richiesto
- **Verifica passo**: output `AQAAAA…`, `len=84`, `selfcheck=Success` ✓

### Fase 2: Aggiorna il DB
- **Stato**: [x]
- `UPDATE AspNetUsers SET PasswordHash = …, SecurityStamp = NEWID(), ConcurrencyStamp = NEWID(), LockoutEnd = NULL, AccessFailedCount = 0 WHERE UserName = 'davraf'`
- Ostacolo incontrato: l'indice unique filtrato `IX_AspNetUsers_NormalizedEmail_Unique` fa fallire l'UPDATE con `Msg 1934 … QUOTED_IDENTIFIER`. Risolto aggiungendo il flag `-I` a sqlcmd.
- **Verifica passo**: `righe aggiornate: 1` ✓

### Fase 3: Verifica login end-to-end
- **Stato**: [x]
- Rilettura hash dal DB → `VerifyHashedPassword` = `Success`; controllo negativo con password errata = `Failed` ✓
- `POST http://localhost:5083/api/v1/auth/login` (307 → HTTPS) → **HTTP 200**, JWT emesso, `"role":"Admin"` ✓

### Fase 4: Disinnesca la migration distruttiva
- **Stato**: [x]
- **Verifica passo**: `grep -i '^\s*DELETE\s+FROM\s+AspNetUsers'` su `schema-migrations/` → nessun match ✓

### Fase 5: Pulizia
- **Stato**: [x]
- **Verifica passo**: `<scratchpad>/pwdhash/` rimossa ✓

## Criteri di verifica finale
- [x] `VerifyHashedPassword` sulla password nuova restituisce `Success`
- [x] `davraf` presente con ruolo `Admin`, non bloccato
- [x] `michela` invariata (prefisso hash immutato, `AQAAAAIAAYagAAAAEBys…`)
- [x] Nessuno `DELETE FROM AspNetUsers` attivo in `schema-migrations/`
- [x] Nessun hash o password in chiaro nei file committati da questo task

## Note di sicurezza
- La password è stata comunicata in chat: se il transcript viene condiviso, va considerata compromessa e ruotata.
- Supera la policy configurata ma resta debole per un account `Admin` (8 caratteri, parola di dizionario + 2 cifre). Valutare una passphrase più lunga.
- **Debito preesistente, fuori scope**: `postgre-compose.yml` (tracciato da git) contiene la stessa password in chiaro alle righe 7, 16 e 22, oltre a un `psql -U Gundam00` che usa la password come nome utente. Da bonificare in un task dedicato — vedi `sensitive-data.instructions.md`.
