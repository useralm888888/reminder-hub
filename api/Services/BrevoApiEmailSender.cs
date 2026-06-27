using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Domain.Entities;
using Api.Options;
using Microsoft.Extensions.Options;

namespace Api.Services;

public class BrevoApiEmailSender(
    IHttpClientFactory httpClientFactory,
    IOptions<BrevoOptions> options,
    ILogger<BrevoApiEmailSender> logger) : IReminderEmailSender
{
    public const string HttpClientName = "Brevo";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly BrevoOptions _options = options.Value;

    public async Task SendAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reminder.Email))
        {
            return;
        }

        if (!_options.IsConfigured)
        {
            logger.LogWarning(
                "Brevo API is not configured. Skipping email for reminder {ReminderId}.",
                reminder.Id);
            return;
        }

        var payload = new BrevoSendEmailRequest(
            new BrevoSender(_options.SenderEmail, _options.SenderName),
            [new BrevoRecipient(reminder.Email)],
            "Reminder",
            reminder.Message);

        var client = httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, "v3/smtp/email")
        {
            Content = JsonContent.Create(payload, options: JsonOptions),
        };
        request.Headers.Add("api-key", _options.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await client.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Brevo API returned {(int)response.StatusCode} {response.ReasonPhrase}: {errorBody}");
        }

        logger.LogInformation(
            "Reminder {ReminderId} emailed to {Email} via Brevo API",
            reminder.Id,
            reminder.Email);
    }

    private sealed record BrevoSendEmailRequest(
        BrevoSender Sender,
        IReadOnlyList<BrevoRecipient> To,
        string Subject,
        string TextContent);

    private sealed record BrevoSender(string Email, string Name);

    private sealed record BrevoRecipient(string Email);
}
