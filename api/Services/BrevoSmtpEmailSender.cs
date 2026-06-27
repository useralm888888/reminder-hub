using Api.Domain.Entities;
using Api.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Api.Services;

public class BrevoSmtpEmailSender : IReminderEmailSender
{
    private readonly BrevoSmtpOptions _options;
    private readonly ILogger<BrevoSmtpEmailSender> _logger;

    public BrevoSmtpEmailSender(
        IOptions<BrevoSmtpOptions> options,
        ILogger<BrevoSmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(reminder.Email))
        {
            return;
        }

        if (!_options.IsConfigured)
        {
            _logger.LogWarning(
                "Brevo SMTP is not configured. Skipping email for reminder {ReminderId}.",
                reminder.Id);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(MailboxAddress.Parse(reminder.Email));
        message.Subject = "Reminder";
        message.Body = new TextPart("plain") { Text = reminder.Message };

        using var client = new SmtpClient
        {
            Timeout = 30_000,
        };
        await client.ConnectAsync(
            _options.SmtpHost,
            _options.SmtpPort,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await client.AuthenticateAsync(_options.Login, _options.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation(
            "Reminder {ReminderId} emailed to {Email}",
            reminder.Id,
            reminder.Email);
    }
}
