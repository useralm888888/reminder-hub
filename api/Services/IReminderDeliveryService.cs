using Api.Domain.Entities;

namespace Api.Services;

public interface IReminderDeliveryService
{
    Task DeliverAsync(Reminder reminder, CancellationToken cancellationToken = default);
}
