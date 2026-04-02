# Autenticazione in Dr.NutrizioNino

## 1. Panoramica

Il sistema di autenticazione si basa su **JWT (JSON Web Token)** trasportato tramite **cookie httpOnly**.

Il frontend non tocca mai il token direttamente. Il browser lo invia automaticamente a ogni richiesta, proteggendo l'applicazione da attacchi XSS.

L'autorizzazione è basata su **ruoli**: `User` e `Admin`. Gli endpoint protetti usano `RequireAuthorization()` o la policy `AdminOnly`.

---

## 2. Stack tecnologico

| Componente | Tecnologia |
|---|---|
| Identity | ASP.NET Core Identity + `UserManager<ApplicationUser>` |
| Ruoli | `IdentityRole<Guid>`, due ruoli: `User` e `Admin` |
| Token | JWT firmato con HMAC-SHA256 |
| Trasporto token | Cookie `auth_token` (httpOnly) |
| Autenticazione backend | `JwtBearer` con lettura da cookie |
| HTTP client frontend | Axios con `withCredentials: true` |
| State management auth | Composable Vue `useAuth` (singleton a livello modulo) |
| Routing guard | `router.beforeEach` in Vue Router |

---

## 3. Flusso di login passo per passo

```
Client (Vue)                  Backend (.NET)
     |                               |
     |  POST /api/v1/auth/login      |
     |  { userName, password }       |
     |------------------------------>|
     |                               | 1. UserManager.FindByNameAsync()
     |                               | 2. CheckPasswordAsync()
     |                               | 3. GetRolesAsync()
     |                               | 4. GenerateJwt() → token HMAC-SHA256
     |                               |    Scadenza: 8 ore
     |  200 OK                       |
     |  Set-Cookie: auth_token=...   |
     |  { userName, role }           |
     |<------------------------------|
     |                               |
     | (stato locale aggiornato)     |
     | user = { userName, role, ... }|
     |                               |
     | GET /api/v1/foods  (richiesta |
     |   successiva — cookie inviato |
     |   automaticamente dal browser)|
     |------------------------------>|
     |                               | JwtBearerEvents.OnMessageReceived:
     |                               |   ctx.Token = cookie["auth_token"]
     |                               | Validazione firma e scadenza
     |  200 OK                       |
     |<------------------------------|
```

### Dettaglio passi

1. Il client invia `POST /api/v1/auth/login` con `{ userName, password }`.
2. `AuthService.LoginAsync` cerca l'utente tramite `UserManager` e verifica la password.
3. Se le credenziali sono valide, genera un JWT con i claim: `sub`, `email`, `name`, `role`, `jti`.
4. Il token viene scritto nel cookie `auth_token` con `HttpOnly = true`. In produzione, `Secure = true`.
5. La risposta restituisce al client solo `{ userName, role }` — il token rimane opaco.
6. `useAuth.login()` popola lo stato locale minimo senza fare una seconda chiamata a `/me`.
7. Ogni richiesta successiva invia il cookie automaticamente. Il middleware `JwtBearer` lo legge dall'evento `OnMessageReceived`.

---

## 4. Come funziona il cookie httpOnly

Il cookie `auth_token` viene impostato dal backend con le seguenti opzioni:

| Opzione | Valore |
|---|---|
| `HttpOnly` | `true` — JavaScript nel browser non può leggerlo |
| `Secure` | `true` in produzione, `false` in sviluppo |
| `SameSite` | `Strict` — il cookie non viene inviato in richieste cross-site |
| `Expires` | Stessa scadenza del JWT (8 ore) |

Il backend legge il cookie nell'evento `OnMessageReceived` di `JwtBearerEvents`, prima della validazione standard. Non è necessario un header `Authorization` nel client.

Il client Axios è configurato con `withCredentials: true` per includere i cookie nelle richieste cross-origin (necessario quando frontend e backend girano su porte diverse in sviluppo).

Per il **logout**, il backend chiama `Response.Cookies.Delete("auth_token")`. Il cookie viene rimosso e le richieste successive risultano non autenticate.

---

## 5. Protezione degli endpoint

### Endpoint che richiedono autenticazione

Usano `.RequireAuthorization()` senza policy specifica. Qualsiasi utente autenticato (ruolo `User` o `Admin`) può accedere.

| Endpoint | Protezione |
|---|---|
| `GET /api/v1/auth/me` | `RequireAuthorization()` |
| `PATCH /api/v1/auth/me/birthdate` | `RequireAuthorization()` |

### Endpoint con policy AdminOnly

Usano `.RequireAuthorization("AdminOnly")`. Solo gli utenti con ruolo `Admin` possono accedere.

La policy è definita in `Program.cs`:

```csharp
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});
```

| Endpoint | Protezione |
|---|---|
| `GET /api/v1/admin/users` | `AdminOnly` |
| `POST /api/v1/admin/users` | `AdminOnly` |
| `PATCH /api/v1/admin/users/{id}/role` | `AdminOnly` |

### Endpoint pubblici

| Endpoint | Note |
|---|---|
| `POST /api/v1/auth/login` | `AllowAnonymous()` |
| `POST /api/v1/auth/logout` | `AllowAnonymous()` |

### Comportamento su 401 e 403

Il middleware Identity è configurato per **non** redirigere a `/Account/Login`. Restituisce direttamente `401 Unauthorized` o `403 Forbidden`. Questo evita conflitti con il flusso JWT.

---

## 6. Frontend: routing guard e gestione sessione

### Navigation guard

Il file `src/router/index.ts` registra un `beforeEach` che protegge tutte le rotte non marcate con `meta: { public: true }`.

```
Navigazione verso rotta protetta
        |
        | to.meta.public? → true → accesso libero
        |
        | false → checkAuth()
               |
               | già verificato? → usa stato in memoria
               |
               | no → GET /api/v1/auth/me
                      | 200 OK → utente autenticato → accesso
                      | errore → redirect a /login?redirect=<path>
```

La rotta `/login` è l'unica marcata `meta: { public: true }`.

### Composable `useAuth`

`useAuth` mantiene uno stato **singleton a livello modulo** (non reattivo globale tramite Pinia, ma un `ref` condiviso fuori dalla funzione). Questo significa che lo stato persiste per tutta la durata della sessione del browser senza reinizializzarsi a ogni navigazione.

| Proprietà/Metodo | Descrizione |
|---|---|
| `user` | Oggetto utente corrente (`MeResponse` o `null`) |
| `isAuthenticated` | `computed` — `true` se `user !== null` |
| `isAdmin` | `computed` — `true` se `user.role === 'Admin'` |
| `checked` | Flag che indica se la verifica iniziale è già avvenuta |
| `checkAuth()` | Chiama `GET /me` una sola volta; usa la cache nelle chiamate successive |
| `login()` | Chiama `POST /login` e popola lo stato locale dalla risposta |
| `logout()` | Chiama `POST /logout`, azzera `user` e `checked` |

### Intercettore Axios (401)

Se il backend risponde con `401` su una rotta che non è `/login`, l'intercettore in `apiClient.ts` reindirizza automaticamente a `/login`, preservando il percorso corrente nel parametro `?redirect=`.

---

## 7. Configurazione

### Backend — `appsettings.json` / `appsettings.local.json`

| Chiave | Descrizione | Sensibile |
|---|---|---|
| `Jwt:Secret` | Chiave simmetrica per la firma HMAC-SHA256 del JWT | Sì — non committare |
| `AllowedOrigins` | Array di origini CORS ammesse (es. `http://localhost:5173`) | No |
| `ConnectionStrings:DrNutrizioNinoSql` | Connection string SQL Server | Sì — non committare |

Il file `appsettings.local.json` non viene committato (ignorato da `.gitignore`). Usarlo per i valori locali sensibili.

Esempio di sezione JWT nel file locale:

```json
{
  "Jwt": {
    "Secret": "<stringa-segreta-lunga-almeno-32-caratteri>"
  }
}
```

### Frontend — `.env.development`

| Variabile | Valore default | Descrizione |
|---|---|---|
| `VITE_API_BASE_URL` | `http://localhost:5083/api/v1` | URL base delle API |

### CORS

La policy CORS `permitGetPost` ammette solo le origini in `AllowedOrigins` e richiede `.AllowCredentials()` per consentire l'invio dei cookie cross-origin in sviluppo.

---

## 8. Possibili Miglioramenti

- **Refresh token**: il JWT scade dopo 8 ore senza possibilità di rinnovo silenzioso. Un refresh token separato permetterebbe sessioni più lunghe senza obbligare al re-login.
- **Revoca token**: i JWT emessi non sono revocabili prima della scadenza. Una blocklist in cache (Redis o in-memory) consentirebbe il logout immediato lato server.
- **Rotazione secret JWT**: attualmente il secret è statico. Una rotazione periodica della chiave aumenterebbe la sicurezza in caso di compromissione.
- **SameSite in modalità sviluppo**: il cookie è `SameSite=Strict`; in sviluppo con domini diversi (es. HTTPS con certificato self-signed) potrebbe essere necessario abbassarlo a `Lax`.
- **Validazione Issuer/Audience**: `ValidateIssuer` e `ValidateAudience` sono disabilitati. Abilitarli riduce il rischio di token accettati da più applicazioni.
- **Rate limiting sul login**: nessuna protezione contro attacchi brute-force sull'endpoint `/login`.
- **Audit log**: le operazioni di login, logout e cambio ruolo non vengono tracciate in un log strutturato dedicato.
- **Multi-ruolo**: l'implementazione attuale assegna un solo ruolo per utente. Supportare più ruoli simultanei richiede modifiche a `GetRolesAsync` e ai claim JWT.

---

*Documento generato il 2026-04-02 — modello `claude-sonnet-4-6`*
