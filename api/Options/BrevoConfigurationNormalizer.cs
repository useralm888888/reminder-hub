namespace Api.Options;

internal static class BrevoConfigurationNormalizer
{
    public static void Apply(BrevoOptions options, IConfiguration configuration)
    {
        options.ApiKey = Trim(options.ApiKey);
        options.SenderEmail = Trim(options.SenderEmail);
        options.SenderName = Trim(options.SenderName);

        if (string.IsNullOrWhiteSpace(options.SenderName))
        {
            options.SenderName = "Reminder Hub";
        }

        var enabledRaw = Trim(configuration[$"{BrevoOptions.SectionName}:Enabled"]);
        if (!string.IsNullOrWhiteSpace(enabledRaw))
        {
            options.Enabled = enabledRaw.Equals("true", StringComparison.OrdinalIgnoreCase)
                || enabledRaw == "1";
        }
    }

    private static string Trim(string? value) =>
        value?.Trim().Trim('"').Trim('\'') ?? string.Empty;
}
