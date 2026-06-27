# Reminder Hub — Angular frontend

Angular 21 SPA for the Reminder Hub assignment. Login, reminder list, scheduling, edit, delete.

The list page connects to the API's SignalR hub on load. When a reminder goes from `Scheduled` to `Sent`, the UI refreshes that page automatically — no manual reload.

## Prerequisites

- Node.js 22+
- Running API (see [root README](../README.md) or [`api/README.md`](../api/README.md))

## Local development

```bash
cd web
npm ci
npm run dev
```

Open `http://localhost:4200`.

Default login: **admin / admin** (configured in `api/appsettings.Development.json`).

## API URL configuration

The frontend does not hardcode the API URL in TypeScript services. It loads runtime config on startup:

| Source | Used when |
|--------|-----------|
| `public/config.json` | Local dev (`ng serve`) |
| `browser/config.json` (build output) | Production Docker / Railway build |

Default local value in `public/config.json`:

```json
{
  "api": {
    "baseUrl": "http://localhost:5169"
  }
}
```

Change `baseUrl` if your API runs on a different port.

### Railway / production

Set on the **frontend** Railway service:

```
API_BASE_URL=https://your-api-service.up.railway.app
```

The Docker build (`npm run build:railway`) writes this into `browser/config.json`.

## Project structure

```
src/app/
├── core/           # Config, services, interceptors, guards
│   └── services/
│       └── reminder-hub.service.ts   # SignalR connection
├── features/
│   ├── auth/       # Login
│   ├── scheduling/ # Create reminder
│   └── reminder-list/
└── layout/         # App shell
```

SignalR client: `@microsoft/signalr`. The hub URL is `{apiBaseUrl}/hubs/reminders`; auth token comes from the same session storage as REST calls.

## Scripts

| Command | Description |
|---------|-------------|
| `npm run dev` | Dev server (`http://localhost:4200`) |
| `npm run build` | Production build → `browser/` |
| `npm run build:railway` | Build + write `config.json` from `API_BASE_URL` |
| `npm test` | Unit tests (Vitest) |

## Deployment

Deployed via Docker + Caddy (see `Dockerfile`, `Caddyfile`, `railway.toml`).

Railway settings:

- **Root directory:** `web`
- **Config file:** `/web/railway.toml`
- **Variable:** `API_BASE_URL` = your API public URL

Ensure the API allows the frontend origin via `CORS_ALLOWED_ORIGINS`. SignalR needs credentials enabled on CORS (already set on the API side).
