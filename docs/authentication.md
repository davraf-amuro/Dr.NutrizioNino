# Autenticazione in Dr.NutrizioNino

## Panoramica

Il sistema di autenticazione si basa su **JWT (JSON Web Token)** trasportato via **localStorage** e inviato come header `Authorization: Bearer <token>` a ogni richiesta.

L'autorizzazione è basata su **ruoli**: `User` e `Admin`. Gli endpoint protetti usano `.RequireAuthorization()` o la policy `AdminOnly`.

> **Nota:** La versione precedente usava cookie `httpOnly`. Il sistema è stato migrato a localStorage + header per semplificare il flusso cross-origin in sviluppo. Le implicazioni di sicurezza sono descritte in fondo.

---

## Stack tecnologico

| Componente | Tecnologia |
|---|---|
| Identity | ASP.NET Core Identity + `UserManager<ApplicationUser>` |
| Ruoli | `IdentityRole<Guid>`, due ruoli: `User` e `Admin` |
| Token | JWT firmato con HMAC-SHA256 |
| Trasporto token | `localStorage` (chiave: `auth_token`) |
| Invio token | Header `Authorization: Bearer <token>` via interceptor Axios |
| Autenticazione backend | Middleware `JwtBearer` standard |
| State management auth | Composable `useAuth` (singleton a livello modulo) |
| Routing guard | `router.beforeEach` in Vue Router |

---

## Flusso di login

```
Client (Vue)                      Backend (.NET)
     |                                   |
     |  POST /api/v1/auth/login          |
     |  { userName, password }           |
     |---------------------------------->|
     |                                   | 1. UserManager.FindByNameAsync()
     |                                   | 2. CheckPasswordAsync()
     |                                   | 3. GetRolesAsync()
     |                                   | 4. GenerateJwt() → token HMAC-SHA256
     |                                   |    Claim: sub, email, name, role, jti
     |                                   |    Scadenza: 8 ore
     |  200 OK                           |
     |  { token, userName, role }        |
     |<----------------------------------|
     |                                   |
     | saveToken(token)                  |
     | → localStorage["auth_token"]      |
     | user = { userName, role, ... }    |
     |                                   |
     | GET /api/v1/foods                 |
     | Authorization: Bearer <token>     |
     |---------------------------------->|
     |                                   | Validazione firma e scadenza
     |  200 OK                           |
     |<----------------------------------|
```

### Passi in dettaglio

1. Il client invia `POST /api/v1/auth/login` con `{ userName, password }`.
2. `AuthService.LoginAsync` verifica le credenziali tramite `UserManager`.
3. Se valide, genera un JWT con i claim: `sub`, `email`, `name`, `role`, `jti`.
4. La risposta restituisce `{ token, userName, role }`.
5. `useAuth.login()` chiama `saveToken(token)` → `localStorage["auth_token"]`.
6. Ogni richiesta successiva invia `Authorization: Bearer <token>` via interceptor Axios.
7. Il middleware `JwtBearer` valida firma e scadenza del token.

---

## Gestione del token nel frontend

### `tokenStorage.ts`

```typescript
const TOKEN_KEY = 'auth_token'

export const saveToken   = (token: string): void => localStorage.setItem(TOKEN_KEY, token)
export const getToken    = (): string | null       => localStorage.getItem(TOKEN_KEY)
export const removeToken = (): void                => localStorage.removeItem(TOKEN_KEY)
```

### Interceptor Axios (`apiClient.ts`)

L'interceptor di request legge il token e lo aggiunge all'header:

```typescript
apiClient.interceptors.request.use((config) => {
  const token = getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})
```

### Validazione locale della scadenza

`useAuth.checkAuth()` decodifica il payload JWT prima di chiamare `/me`:

```typescript
function isTokenValid(token: string): boolean {
  const payload = JSON.parse(atob(token.split('.')[1]))
  return typeof payload.exp === 'number' && payload.exp * 1000 > Date.now()
}
```

Se il token è scaduto, viene rimosso dal localStorage senza fare una chiamata API.

---

## Logout

Il backend restituisce `204 No Content` senza logica server-side: il token JWT non viene revocato (vedi limitazioni).

Il frontend gestisce il logout in `useAuth.logout()`:
1. Chiama `POST /api/v1/auth/logout` (per compatibilità futura).
2. Chiama `removeToken()` → rimuove `localStorage["auth_token"]`.
3. Azzera `user` e il flag `checked`.

---

## Composable `useAuth`

Mantiene uno stato **singleton a livello modulo** (ref condivisi fuori dalla funzione), persistente per tutta la sessione del browser.

| Proprietà/Metodo | Descrizione |
|---|---|
| `user` | Oggetto utente corrente (`MeResponse` o `null`) |
| `isAuthenticated` | `computed` — `true` se `user !== null` |
| `isAdmin` | `computed` — `true` se `user.role === 'Admin'` |
| `checked` | Flag: la verifica iniziale è già avvenuta |
| `checkAuth()` | Valida il token localmente; chiama `GET /me` una sola volta |
| `login()` | Chiama `POST /login`, salva il token, popola lo stato |
| `logout()` | Chiama `POST /logout`, rimuove il token, azzera lo stato |
| `resetAuth()` | Rimuove il token e azzera lo stato senza chiamate API (usato dall'interceptor 401) |

---

## Navigation guard

Il file `src/router/index.ts` registra un `beforeEach` che protegge le rotte non pubbliche.

```
Navigazione verso rotta protetta
        |
        | to.meta.public? → true → accesso libero
        |
        | false → checkAuth()
               |
               | token valido + già verificato? → usa stato in memoria
               |
               | token valido + non verificato → GET /api/v1/auth/me
               |        200 OK → accesso
               |        errore  → redirect a /login?redirect=<path>
               |
               | to.meta.requiresAdmin? → isAdmin? no → redirect a /
               |
               | token assente o scaduto → redirect a /login
```

La rotta `/login` è l'unica marcata `meta: { public: true }`.
La rotta `/admin/users` è marcata `meta: { requiresAdmin: true }`.

---

## Protezione degli endpoint backend

### Autenticazione standard

Qualsiasi utente con token valido (`User` o `Admin`).

| Endpoint | Metodo | Note |
|---|---|---|
| `GET /api/v1/auth/me` | `RequireAuthorization()` | Dati utente corrente |
| `PATCH /api/v1/auth/me/birthdate` | `RequireAuthorization()` | Aggiorna data di nascita |

### Policy AdminOnly

Solo `Admin`. Definita in `Program.cs`:
```csharp
opt.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
```

| Endpoint | Metodo |
|---|---|
| `GET /api/v1/admin/users` | Elenco utenti |
| `POST /api/v1/admin/users` | Crea utente |
| `GET /api/v1/admin/users/{id}` | Dettaglio utente |
| `PUT /api/v1/admin/users/{id}` | Aggiorna utente |
| `DELETE /api/v1/admin/users/{id}` | Elimina utente |
| `PATCH /api/v1/admin/users/{id}/role` | Cambia ruolo |

### Endpoint pubblici

| Endpoint | Note |
|---|---|
| `POST /api/v1/auth/login` | `AllowAnonymous()` |
| `POST /api/v1/auth/logout` | `AllowAnonymous()` |

### Comportamento su 401 e 403

Il middleware Identity non redirige a `/Account/Login`. Restituisce `401` o `403` direttamente.

L'interceptor Axios su 401 chiama `resetAuth()` e redirige a `/login?redirect=<path>&reason=session_expired`.

---

## Configurazione

### Backend — `appsettings.json` / `appsettings.local.json`

| Chiave | Descrizione | Sensibile |
|---|---|---|
| `Jwt:Secret` | Chiave simmetrica HMAC-SHA256 (min. 32 caratteri) | **Sì** |
| `AllowedOrigins` | Array origini CORS (es. `http://localhost:5173`) | No |
| `ConnectionStrings:DrNutrizioNinoSql` | Connection string SQL Server | **Sì** |

Il file `appsettings.local.json` non viene committato.

### Frontend — `.env.development`

| Variabile | Default | Descrizione |
|---|---|---|
| `VITE_API_BASE_URL` | `http://localhost:5083/api/v1` | URL base API |

### CORS

La policy CORS ammette le origini in `AllowedOrigins`. Non richiede più `.AllowCredentials()` perché i cookie non sono in uso.

---

## Considerazioni di sicurezza

| Aspetto | Stato attuale | Impatto |
|---|---|---|
| XSS e localStorage | Token leggibile da JS — rischio se XSS presente | Medio |
| Revoca token | I JWT emessi non sono revocabili prima della scadenza | Medio |
| Refresh token | Nessuno — re-login manuale dopo 8 ore | Basso |
| Brute-force login | Nessun rate limiting sull'endpoint `/login` | Medio |
| Issuer/Audience | `ValidateIssuer` e `ValidateAudience` disabilitati | Basso |
| Audit log | Login/logout/cambio ruolo non tracciati separatamente | Basso |

---

*Documento aggiornato il 2026-04-03 — modello `claude-sonnet-4-6`*
