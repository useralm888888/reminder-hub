using Api.Domain.Entities;
using Api.Domain.Enums;

namespace Api.Repositories;

public interface IReminderRepository
{
    Task<Reminder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Reminder> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<(int ScheduledCount, int SentCount)> GetStatusCountsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Reminder>> GetDueRemindersAsync(
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default);

    Task AddAsync(Reminder reminder, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Reminder reminder, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> TryMarkAsSentAsync(
        Guid id,
        DateTimeOffset sentAt,
        CancellationToken cancellationToken = default);
}
