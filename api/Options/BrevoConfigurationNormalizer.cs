namespace Api.Options;

internal static class BrevoConfigurationNormalizer
{
    public static void Apply(BrevoSmtpOptions options, IConfiguration configuration)
    {
        options.Login = Trim(options.Login);
        options.Password = Trim(options.Password);
        options.SenderEmail = Trim(options.SenderEmail);
        options.SenderName = Trim(options.SenderName);
        options.SmtpHost = Trim(options.SmtpHost);

        if (string.IsNullOrWhiteSpace(options.SmtpHost))
        {
            options.SmtpHost = "smtp-relay.brevo.com";
        }

        var enabledRaw = Trim(configuration[$"{BrevoSmtpOptions.SectionName}:Enabled"]);
        if (!string.IsNullOrWhiteSpace(enabledRaw))
        {
            options.Enabled = enabledRaw.Equals("true", StringComparison.OrdinalIgnoreCase)
                || enabledRaw == "1";
        }
    }

    private static string Trim(string? value) =>
        value?.Trim().Trim('"').Trim('\'') ?? string.Empty;
}
