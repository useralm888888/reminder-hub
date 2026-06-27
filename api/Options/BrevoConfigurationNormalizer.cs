namespace Api.Options;

internal static class BrevoConfigurationNormalizer
{
    public static void Apply(BrevoOptions options, IConfiguration configuration)
    {
        var section = configuration.GetSection(BrevoOptions.SectionName);

        options.ApiKey = Trim(section["ApiKey"]);
        options.SenderEmail = Trim(section["SenderEmail"]);
        options.SenderName = Trim(section["SenderName"]);

        if (string.IsNullOrWhiteSpace(options.SenderName))
        {
            options.SenderName = "Reminder Hub";
        }

        var enabledRaw = Trim(section["Enabled"]);
        options.Enabled = enabledRaw.Equals("true", StringComparison.OrdinalIgnoreCase)
            || enabledRaw == "1";
    }

    private static string Trim(string? value) =>
        value?.Trim().Trim('"').Trim('\'') ?? string.Empty;
}
