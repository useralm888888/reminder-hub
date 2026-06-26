namespace Api.Options;

public class BrevoSmtpOptions
{
    public const string SectionName = "Brevo";

    public bool Enabled { get; set; }

    public string SmtpHost { get; set; } = "smtp-relay.brevo.com";

    public int SmtpPort { get; set; } = 587;

    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = "Reminder Hub";

    public bool IsConfigured =>
        Enabled
        && !string.IsNullOrWhiteSpace(Login)
        && !string.IsNullOrWhiteSpace(Password)
        && !string.IsNullOrWhiteSpace(SenderEmail);
}
