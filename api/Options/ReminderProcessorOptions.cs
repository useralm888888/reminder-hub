namespace Api.Options;

public class ReminderProcessorOptions
{
    public const string SectionName = "ReminderProcessor";

    public int PollingIntervalSeconds { get; set; } = 15;
}
