using Api.Domain.Entities;

namespace Api.Services;

public class CompositeReminderDeliveryService(
    FileReminderDeliveryService fileDelivery,
    IReminderEmailSender emailSender) : IReminderDeliveryService
{
    public async Task DeliverAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        await fileDelivery.DeliverAsync(reminder, cancellationToken);

        if (!string.IsNullOrWhiteSpace(reminder.Email))
        {
            await emailSender.SendAsync(reminder, cancellationToken);
        }
    }
}
