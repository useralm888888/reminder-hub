namespace Api.Options;

public class BrevoOptions
{
    public const string SectionName = "Brevo";

    public bool Enabled { get; set; }

    public string ApiKey { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = "Reminder Hub";

    public bool IsConfigured =>
        Enabled
        && !string.IsNullOrWhiteSpace(ApiKey)
        && !string.IsNullOrWhiteSpace(SenderEmail);
}
