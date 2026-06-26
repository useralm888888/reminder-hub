namespace Api.Options;

public class ReminderDeliveryOptions
{
    public const string SectionName = "ReminderDelivery";

    public string LogFilePath { get; set; } = "logs/reminders.log";
}
