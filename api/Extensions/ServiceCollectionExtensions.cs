using System.Text.Json.Serialization;
using Api.Authentication;
using Api.Background;
using Api.Data;
using Api.Options;
using Microsoft.AspNetCore.Authentication;
using Api.Repositories;
using Api.Services;
using Api.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.Configure<ReminderProcessorOptions>(
            configuration.GetSection(ReminderProcessorOptions.SectionName));
        services.Configure<ReminderDeliveryOptions>(
            configuration.GetSection(ReminderDeliveryOptions.SectionName));
        services.AddOptions<BrevoOptions>()
            .Bind(configuration.GetSection(BrevoOptions.SectionName))
            .PostConfigure(options => BrevoConfigurationNormalizer.Apply(options, configuration));
        services.Configure<ApiAuthOptions>(
            configuration.GetSection(ApiAuthOptions.SectionName));
        services.Configure<AuthOptions>(
            configuration.GetSection(AuthOptions.SectionName));

        services.AddScoped<IAuthService, AuthService>();

        services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationDefaults.AuthenticationScheme,
                _ => { });
        services.AddAuthorization();

        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddSingleton<IReminderNotifier, SignalRReminderNotifier>();

        services.AddSingleton<FileReminderDeliveryService>();
        services.AddHttpClient(BrevoApiEmailSender.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://api.brevo.com/");
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddSingleton<IReminderEmailSender, BrevoApiEmailSender>();
        services.AddSingleton<IReminderDeliveryService>(sp =>
        {
            var brevoOptions = sp.GetRequiredService<IOptions<BrevoOptions>>().Value;
            var fileDelivery = sp.GetRequiredService<FileReminderDeliveryService>();

            if (!brevoOptions.IsConfigured)
            {
                return fileDelivery;
            }

            return new CompositeReminderDeliveryService(
                fileDelivery,
                sp.GetRequiredService<IReminderEmailSender>(),
                sp.GetRequiredService<ILogger<CompositeReminderDeliveryService>>());
        });
        services.AddHostedService<ReminderProcessorHostedService>();

        services.AddValidatorsFromAssemblyContaining<CreateReminderRequestValidator>();
        services.AddFluentValidationAutoValidation();

        if (configuration.GetValue("HealthChecks:Enabled", true))
        {
            services.AddHealthChecks()
                .AddNpgSql(connectionString, name: "postgresql");
        }

        return services;
    }

    public static IMvcBuilder AddJsonSerialization(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }
}
