using Api.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        var applyMigrations = app.Configuration.GetValue(
            "Database:ApplyMigrationsOnStartup",
            app.Environment.IsProduction());

        if (!applyMigrations)
        {
            return;
        }

        var maxRetries = app.Configuration.GetValue("Database:MigrationRetryCount", 10);
        var delaySeconds = app.Configuration.GetValue("Database:MigrationRetryDelaySeconds", 3);
        var logger = app.Services
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseMigration");

        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                logger.LogInformation(
                    "Applying database migrations (attempt {Attempt}/{MaxRetries})...",
                    attempt,
                    maxRetries);

                await dbContext.Database.MigrateAsync();
                await EnsureReminderVersionColumnAsync(dbContext, logger);
                logger.LogInformation("Database migrations applied successfully.");
                return;
            }
            catch (Exception ex) when (IsTransientConnectionError(ex))
            {
                lastException = ex;

                if (attempt == maxRetries)
                {
                    break;
                }

                logger.LogWarning(
                    ex,
                    "PostgreSQL is not reachable yet. Retrying in {DelaySeconds}s...",
                    delaySeconds);

                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }

        throw new InvalidOperationException(
            """
            Could not connect to PostgreSQL. Ensure the database is running before starting the API:
              1. Start Docker Desktop
              2. cd api
              3. docker compose up -d postgres
              4. dotnet run --project Api.csproj
            """,
            lastException);
    }

    private static async Task EnsureReminderVersionColumnAsync(
        AppDbContext dbContext,
        ILogger logger)
    {
        if (!dbContext.Database.IsRelational())
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE reminders
            ADD COLUMN IF NOT EXISTS "Version" integer NOT NULL DEFAULT 0;
            """);

        logger.LogInformation("Verified reminders.Version column exists.");
    }

    private static bool IsTransientConnectionError(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is NpgsqlException or TimeoutException)
            {
                return true;
            }
        }

        return false;
    }
}
