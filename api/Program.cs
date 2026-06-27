using Api.Extensions;
using Api.Hubs;
using Api.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

var corsOrigins = GetCorsOrigins(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var policyBuilder = policy.AllowAnyHeader().AllowAnyMethod();

        if (corsOrigins.Length > 0)
        {
            policyBuilder.WithOrigins(corsOrigins).AllowCredentials();
        }
        else
        {
            policyBuilder.SetIsOriginAllowed(_ => true).AllowCredentials();
        }
    });
});

builder.Services.AddControllers()
    .AddJsonSerialization();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reminder API",
        Version = "v1",
        Description = "API for scheduling, viewing, and delivering reminders."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "API token",
        In = ParameterLocation.Header,
        Description = "Schedule token required for POST /reminders."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

LogBrevoConfiguration(app);

await app.ApplyDatabaseMigrationsAsync();

app.UseExceptionHandler();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Reminder API v1");
    options.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

if (app.Configuration.GetValue("HealthChecks:Enabled", true))
{
    app.MapHealthChecks("/health");
}

app.MapControllers();
app.MapHub<RemindersHub>("/hubs/reminders");

app.Run();

static void LogBrevoConfiguration(WebApplication app)
{
    var brevo = app.Services.GetRequiredService<IOptions<BrevoSmtpOptions>>().Value;
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    if (!brevo.Enabled)
    {
        logger.LogWarning(
            "Brevo email is disabled (Brevo:Enabled=false). Reminders will be marked Sent after file log only.");
        return;
    }

    if (!brevo.IsConfigured)
    {
        logger.LogWarning(
            "Brevo is enabled but incomplete. Login set={HasLogin}, Password set={HasPassword}, SenderEmail set={HasSenderEmail}",
            !string.IsNullOrWhiteSpace(brevo.Login),
            !string.IsNullOrWhiteSpace(brevo.Password),
            !string.IsNullOrWhiteSpace(brevo.SenderEmail));
        return;
    }

    logger.LogInformation(
        "Brevo email delivery active. SMTP={Host}:{Port}, Sender={SenderEmail}",
        brevo.SmtpHost,
        brevo.SmtpPort,
        brevo.SenderEmail);
}

static string[] GetCorsOrigins(IConfiguration configuration)
{
    var envOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
    if (!string.IsNullOrWhiteSpace(envOrigins))
    {
        return envOrigins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
    if (!string.IsNullOrWhiteSpace(frontendUrl))
    {
        return [frontendUrl.Trim().TrimEnd('/')];
    }

    return configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
}

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                ? exception.Message
                : null
        };

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

public partial class Program { }
