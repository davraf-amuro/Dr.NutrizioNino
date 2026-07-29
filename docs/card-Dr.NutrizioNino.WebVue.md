# Card: Dr.NutrizioNino.WebVue

## Identificazione
- **Progetto:** Dr.NutrizioNino.WebVue (`dr-nutrizionino-webvue`)
- **Solution:** Dr.NutrizioNino.sln (il progetto frontend non è incluso nella solution .NET)
- **Workspace:** —
- **Repository:** https://github.com/davraf-amuro/Dr.NutrizioNino
- **Tipo Applicazione:** Frontend SPA
- **Pattern Architetturale:** Vue 3 Composition API + moduli per dominio (`api/` + `composables/`)
- **Versione Corrente:** 0.0.0
- **Owner/Team:**
- **Referente:** Davide Raffagli
- **Contatto Supporto:**

## Stack Tecnologico
- **Linguaggio Principale:** TypeScript 5.9
- **Framework:** Vue 3.5
- **Build tool:** Vite 8
- **UI kit:** Naive UI 2.44

## Dipendenze

### Progetti Interni
- `Dr.NutrizioNino.Api` — backend consumato via HTTP
- Le interfacce TypeScript in `src/Interfaces/` rispecchiano i DTO di `Dr.NutrizioNino.Models`

### Pacchetti Esterni
| Pacchetto | Versione | Scopo |
|-----------|----------|-------|
| vue | ^3.5.30 | Framework UI |
| vue-router | ^5.0.3 | Routing e navigation guard |
| axios | ^1.13.6 | Client HTTP con interceptor JWT |
| chart.js | ^4.5.1 | Grafici nutrienti |
| vue-chartjs | ^5.3.3 | Wrapper Vue per Chart.js |
| chartjs-plugin-annotation | ^3.1.0 | Soglie e annotazioni sui grafici |
| vue-draggable-plus | ^0.6.1 | Riordino drag & drop (nutrienti, sezioni) |
| naive-ui | ^2.44.1 | Componenti UI |
| vite | ^8.0.0 | Dev server e build |
| typescript | ~5.9.3 | Tipizzazione statica |
| vue-tsc | ^3.2.5 | Type-check dei componenti `.vue` |
| eslint | ^10.0.3 | Lint |
| prettier | ^3.8.1 | Formattazione |

## Struttura del codice

| Cartella | Contenuto |
|----------|-----------|
| `src/components/<Dominio>/` | Componenti UI per dominio: Admin, Brands, Categories, DailySimulations, Dishes, Foods, Nutrients, Supermarkets, Units, icons |
| `src/modules/<feature>/api/` | Chiamate HTTP verso il backend, una cartella per feature |
| `src/modules/<feature>/composables/` | Stato e logica riusabile della feature |
| `src/Interfaces/<dominio>/` | Tipi TypeScript allineati ai DTO del backend |
| `src/core/http/` | `apiClient` Axios, interceptor, `ApiError` |
| `src/core/composables/` | Composable trasversali (es. `useAsyncState`) |
| `src/core/utils/` | Helper generici |
| `src/views/` | Pagine associate alle route |
| `src/router/` | Definizione route e navigation guard |

## Servizi Esterni
| Tipo | Nome/Endpoint | Protocollo | Autenticazione | Scopo/Descrizione |
|------|---------------|------------|----------------|-------------------|
| API interna | Dr.NutrizioNino.Api — `VITE_API_BASE_URL` | HTTP/HTTPS | JWT Bearer da localStorage | Tutte le operazioni di dominio |

## Comandi

| Comando | Effetto |
|---------|---------|
| `npm run dev` | Dev server Vite |
| `npm run build` | Type-check + build di produzione |
| `npm run type-check` | Solo `vue-tsc --build --force` |
| `npm run lint` | ESLint con `--fix` — gate obbligatorio prima della push |
| `npm run format` | Prettier su `src/` |

## Configurazione e Hosting
- **Entrypoint:** `src/Dr.NutrizioNino.WebVue/src/main.ts`
- **Variabile di configurazione:** `VITE_API_BASE_URL` (file `.env.development`)
- **Ambiente Test:** non pubblicato
- **Ambiente Produzione:** non pubblicato

---
*Revisione v1.0 — 2026-07-29 22:24 — claude-opus-5*
