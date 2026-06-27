using Api.Domain.Enums;

namespace Api.Services;

public interface IReminderNotifier
{
    Task NotifyStatusChangedAsync(
        Guid reminderId,
        ReminderStatus status,
        CancellationToken cancellationToken = default);
}
