# Reminder Hub

Full-stack reminder app: .NET 8 API + Angular frontend + PostgreSQL. Schedule a message, the background worker delivers it (file log + optional Brevo email), and the UI updates over SignalR when status changes to `Sent`.

Details: [`api/README.md`](api/README.md) (API, Brevo, migrations) · [`web/README.md`](web/README.md) (Angular, config, deploy)

## Run locally

**1. API + Postgres**

Config files under `api/` are gitignored. Copy the example and adjust if needed:

```bash
cp api/appsettings.Development.example.json api/appsettings.Development.json
```

```bash
cd api
docker compose up -d postgres
dotnet run
```

API: `http://localhost:5169` · Swagger: `/swagger`

**2. Frontend**

```bash
cd web
npm ci
npm run dev
```

Open `http://localhost:4200`. Login: **admin / admin** (set in `api/appsettings.Development.json`).

## Run with Docker

Full stack (Postgres + API + Angular) from the repo root:

```bash
docker compose up --build
```

| Service | URL |
|---------|-----|
| Web | http://localhost:4200 |
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| Postgres | `localhost:5443` (user `reminder`, password `reminder`, db `reminders`) |

Login: **admin / admin**. The API schedule token is set via `ApiAuth__ScheduleToken` in `docker-compose.yml` (default: `docker-schedule-token-change-me`).

Stop and remove containers:

```bash
docker compose down
```

**API + Postgres only** (no frontend container):

```bash
docker compose -f api/docker-compose.yml up --build
```

API: http://localhost:8080 · run the Angular app separately with `npm run dev` in `web/`.

## Railway

Two services + Postgres:

| Service | Root dir | Key env vars |
|---------|----------|--------------|
| API | `api` | `ConnectionStrings__DefaultConnection`, `CORS_ALLOWED_ORIGINS`, `ApiAuth__ScheduleToken`, optional `Brevo__*` |
| Web | `web` | `API_BASE_URL` = public API URL |

Point `CORS_ALLOWED_ORIGINS` at the web URL (e.g. `https://reminder-hub.up.railway.app`).

For Brevo on Railway use the HTTP API key (`xkeysib-...`), not SMTP — Hobby blocks port 587. See [`api/README.md`](api/README.md#brevo-optional).
