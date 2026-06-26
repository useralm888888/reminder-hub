# Reminder Hub API

.NET 8 Web API for scheduling, viewing, and automatically delivering reminders. Data is persisted in PostgreSQL; due reminders are processed by a background worker and logged to a file.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for PostgreSQL)

## Quick start (local development)

### 1. Start PostgreSQL

```bash
cd api
docker compose up -d postgres
```

### 2. Run the API

```bash
cd api
dotnet run
```

The API applies EF Core migrations on startup in Development (`appsettings.Development.json`).

- Swagger UI: `https://localhost:7xxx/swagger` or `http://localhost:5xxx/swagger` (see console output)
- Health check: `GET /health`

### 3. Run with Docker (API + PostgreSQL)

From the repository root:

```bash
docker compose -f api/docker-compose.yml up --build
```

API: `http://localhost:8080`  
Swagger: `http://localhost:8080/swagger`

## API endpoints

### Create a reminder

`POST /reminders` — **requires** `Authorization: Bearer <schedule-token>`

```bash
curl -X POST http://localhost:5169/reminders \
  -H "Authorization: Bearer dev-schedule-token-change-me" \
  -H "Content-Type: application/json" \
  -d '{"message":"Check API gateway logs","sendAt":"2026-06-27T14:30:00Z","email":"test@example.com"}'
```

```json
{
  "message": "Check API gateway logs",
  "sendAt": "2026-06-27T14:30:00Z",
  "email": "test@example.com"
}
```

Response (`201 Created`):

```json
{
  "id": "generated-guid",
  "status": "Scheduled",
  "sendAt": "2026-06-27T14:30:00Z"
}
```

### List all reminders

`GET /reminders`

```json
[
  {
    "id": "...",
    "message": "Check logs",
    "sendAt": "2026-06-27T14:30:00Z",
    "status": "Scheduled",
    "email": "test@example.com"
  }
]
```

## Architecture

```
Controller → Service (business logic) → Repository → EF Core → PostgreSQL
                                              ↑
                              Background worker (polling)
                                              ↓
                              Delivery service (file + console log)
```

### Layers

| Layer | Responsibility |
|-------|----------------|
| **Controller** | HTTP only — delegates to `IReminderService` |
| **Service** | Business rules, orchestrates create/list/process flows |
| **Repository** | Data access queries, idempotent status updates |
| **Background worker** | Polls every 15s for due reminders |
| **Delivery service** | Writes `[timestamp] Reminder sent: ...` to log file and `ILogger` |

### Design decisions

- **PostgreSQL** — reminders survive restarts; suitable for a small production-style service.
- **Repository + Unit of Work** — keeps EF Core out of the service layer; services depend on abstractions.
- **Polling background worker** — simple and sufficient for the assignment scope (no Hangfire/Quartz dependency).
- **Idempotent processing** — `MarkAsSentAsync` uses `UPDATE ... WHERE status = Scheduled` so a reminder is never sent twice.
- **Strategy-ready delivery** — `IReminderDeliveryService` composes file logging with optional Brevo SMTP email when configured.
- **FluentValidation** — request validation lives outside controllers.
- **Structured logging** — all key events use `ILogger` with named properties.

### Database schema

Table `reminders`:

| Column | Type | Notes |
|--------|------|-------|
| `Id` | uuid | Primary key |
| `Message` | varchar(500) | Required |
| `SendAt` | timestamptz | UTC scheduled time |
| `Email` | varchar(320) | Optional |
| `Status` | varchar(20) | `Scheduled` / `Sent` |
| `CreatedAt` | timestamptz | Audit |
| `SentAt` | timestamptz | Set when delivered |

Index on `(Status, SendAt)` for efficient due-reminder queries.

## Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `ConnectionStrings:DefaultConnection` | — | PostgreSQL connection string |
| `Database:ApplyMigrationsOnStartup` | `false` | Auto-run EF migrations on startup |
| `ReminderProcessor:PollingIntervalSeconds` | `15` | Background poll interval |
| `ReminderDelivery:LogFilePath` | `logs/reminders.log` | Delivery log file path |
| `Brevo:Enabled` | `false` | Enable Brevo SMTP email delivery |
| `Brevo:SmtpHost` | `smtp-relay.brevo.com` | Brevo SMTP server |
| `Brevo:SmtpPort` | `587` | Brevo SMTP port (STARTTLS) |
| `Brevo:Login` | — | Brevo SMTP login |
| `Brevo:Password` | — | Brevo SMTP key (not your account password) |
| `Brevo:SenderEmail` | — | Verified sender address in Brevo |
| `Brevo:SenderName` | `Reminder Hub` | Display name for outgoing emails |
| `HealthChecks:Enabled` | `true` | Enable `/health` endpoint |
| `CORS_ALLOWED_ORIGINS` | — | Comma-separated origins (env var) |
| `ApiAuth:ScheduleToken` | — | Bearer token required for `POST /reminders` |

## Assumptions

- `sendAt` must be in the **future** (UTC). Past times are rejected with `400 Bad Request`.
- `email` is **optional**; when provided and Brevo is configured, a real email is sent in addition to file logging.
- When Brevo is disabled or not configured, delivery falls back to **file + console log only**.
- Single API instance — no distributed locking; idempotent DB updates prevent duplicate sends within one instance.
- Failed deliveries are **retried** on the next polling cycle; status stays `Scheduled` until delivery succeeds.

## EF Core migrations

Create a new migration after model changes:

```bash
cd api
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Manual test flow

1. `POST /reminders` with `sendAt` 1–2 minutes in the future.
2. `GET /reminders` — status should be `Scheduled`.
3. Wait for the scheduled time (+ up to 15s polling interval).
4. `GET /reminders` again — status should be `Sent`.
5. Check `api/logs/reminders.log` for the delivery entry.
6. If Brevo is configured and `email` was set, check the inbox for the reminder email.

## Brevo SMTP setup (optional)

Brevo free plan includes **300 emails/day** and works with this API via SMTP.

1. Create a free account at [brevo.com](https://www.brevo.com).
2. Verify a **sender email** under *Senders, Domains & Dedicated IPs*.
3. Generate an **SMTP key** under *Settings → SMTP & API → SMTP keys* (this is the password, not your Brevo login password).
4. Configure credentials via **User Secrets** (recommended — never commit secrets):

```bash
cd api
dotnet user-secrets set "Brevo:Enabled" "true"
dotnet user-secrets set "Brevo:Login" "b00175001@smtp-brevo.com"
dotnet user-secrets set "Brevo:Password" "YOUR_SMTP_KEY_HERE"
dotnet user-secrets set "Brevo:SenderEmail" "your-verified@email.com"
```

Or via environment variables: `Brevo__Enabled`, `Brevo__Login`, `Brevo__Password`, `Brevo__SenderEmail`.

When `Brevo:Enabled` is `true` and all required fields are set, reminders with an `email` address are sent via SMTP **and** logged to file.
