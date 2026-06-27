# Reminder Hub API

.NET 8 Web API — the core of the assignment. Schedules reminders in PostgreSQL, delivers them via a background worker.

See the [root README](../README.md) for full-stack setup and deployment.

## Run locally

`appsettings*.json` files are gitignored. Create `appsettings.Development.json` locally (see [root README](../README.md) for a starter template).

```bash
cd api
docker compose up -d postgres
dotnet run
```

Swagger: `/swagger`  
Health: `/health`

Docker (API + Postgres):

```bash
docker compose -f api/docker-compose.yml up --build
```

## Endpoints

**POST /reminders** — create (needs `Authorization: Bearer <schedule-token>`)

```bash
curl -X POST http://localhost:5169/reminders \
  -H "Authorization: Bearer dev-schedule-token-change-me" \
  -H "Content-Type: application/json" \
  -d '{"message":"Check API gateway logs","sendAt":"2026-06-27T14:30:00Z","email":"test@example.com"}'
```

**GET /reminders** — list (paginated: `?page=1&pageSize=20`, needs Bearer token)

Also: `PUT /reminders/{id}`, `DELETE /reminders/{id}`, `POST /auth/login` for the Angular UI.

**SignalR** — `GET /hubs/reminders` (WebSocket). Event `ReminderStatusChanged` with `{ id, status }` when a reminder is marked sent. Same Bearer token as REST (header or `access_token` query param for the WS upgrade).

## Structure

```
Controller → Service → Repository → EF Core → PostgreSQL (reminders table)
                         ↑
              Background worker (15s poll)
                         ↓
              File log (+ optional Brevo SMTP)
                         ↓
              SignalR push → Angular list refresh
```

Validation is in FluentValidation. Business logic stays in `ReminderService`, data access in `ReminderRepository`.

### Database

Single entity: `Reminder` (`Domain/Entities/Reminder.cs`). Mapped to table `reminders` with an index on `(Status, SendAt)` so the worker can find due items quickly. Migrations live in `Migrations/` — `InitialCreate` sets up the schema.

## Config

Important settings (env vars use `__` instead of `:`):

| Setting | Notes |
|---------|-------|
| `ConnectionStrings__DefaultConnection` | Postgres |
| `ReminderProcessor__PollingIntervalSeconds` | default 15 |
| `ReminderDelivery__LogFilePath` | default `logs/reminders.log` |
| `ApiAuth__ScheduleToken` | Bearer token for all `/reminders` endpoints + SignalR hub |
| `Brevo__Enabled` | `false` by default |
| `Brevo__Login`, `Brevo__Password`, `Brevo__SenderEmail` | SMTP credentials |

Full defaults are in `appsettings.json`.

## Brevo (optional)

The assignment doesn't require real email. If you want it anyway:

1. Verify a sender in Brevo
2. Create an SMTP key (not your account password)
3. Set via user secrets locally:

```bash
dotnet user-secrets set "Brevo:Enabled" "true"
dotnet user-secrets set "Brevo:Login" "your-login@smtp-brevo.com"
dotnet user-secrets set "Brevo:Password" "your-smtp-key"
dotnet user-secrets set "Brevo:SenderEmail" "your-verified@email.com"
```

## Migrations

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Tests

```bash
dotnet test api/ReminderApi.sln
```
