using Api.Domain.Entities;

namespace Api.Services;

public class CompositeReminderDeliveryService(
    FileReminderDeliveryService fileDelivery,
    IReminderEmailSender emailSender,
    ILogger<CompositeReminderDeliveryService> logger) : IReminderDeliveryService
{
    public async Task DeliverAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        await fileDelivery.DeliverAsync(reminder, cancellationToken);

        if (string.IsNullOrWhiteSpace(reminder.Email))
        {
            return;
        }

        try
        {
            await emailSender.SendAsync(reminder, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(
                ex,
                "Failed to send email for reminder {ReminderId}. File log delivery succeeded.",
                reminder.Id);
        }
    }
}
