using Api.Domain.Entities;

namespace Api.Services;

public interface IReminderEmailSender
{
    Task SendAsync(Reminder reminder, CancellationToken cancellationToken = default);
}
