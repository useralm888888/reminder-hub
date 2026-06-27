using Api.Domain.Enums;
using Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Api.Services;

public class SignalRReminderNotifier(IHubContext<RemindersHub> hubContext) : IReminderNotifier
{
    public Task NotifyStatusChangedAsync(
        Guid reminderId,
        ReminderStatus status,
        CancellationToken cancellationToken = default)
    {
        return hubContext.Clients.All.SendAsync(
            RemindersHub.ReminderStatusChangedMethod,
            new { id = reminderId, status = status.ToString() },
            cancellationToken);
    }
}
