# Dr.NutrizioNino.WebVue

This template should help get you started developing with Vue 3 in Vite.

## Recommended IDE Setup

[VSCode](https://code.visualstudio.com/) + [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) (and disable Vetur).

## Type Support for `.vue` Imports in TS

TypeScript cannot handle type information for `.vue` imports by default, so we replace the `tsc` CLI with `vue-tsc` for type checking. In editors, we need [Volar](https://marketplace.visualstudio.com/items?itemName=Vue.volar) to make the TypeScript language service aware of `.vue` types.

## Customize configuration

See [Vite Configuration Reference](https://vitejs.dev/config/).

## Project Setup

```sh
npm install
```

### Environment configuration

Set API endpoint via Vite env variable:

```sh
VITE_API_BASE_URL=http://localhost:5083
```

Available files:

- `.env.development`
- `.env.production`
- `.env.example`

## Frontend architecture

- `src/core/http/`: shared HTTP client and interceptors
- `src/modules/<domain>/api/`: domain API calls
- `src/modules/<domain>/composables/`: domain state and orchestration
- `src/components/`: presentational UI components

### Compile and Hot-Reload for Development

```sh
npm run dev
```

### Type-Check, Compile and Minify for Production

```sh
npm run build
```

### Lint with [ESLint](https://eslint.org/)

```sh
npm run lint
```
