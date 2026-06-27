using Api.Data;
using Api.Domain.Entities;
using Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class ReminderRepository(AppDbContext context) : IReminderRepository
{
    public async Task<Reminder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Reminders
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Reminder>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Reminders
            .AsNoTracking()
            .OrderBy(r => r.SendAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Reminder> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.Reminders.AsNoTracking().OrderByDescending(r => r.SendAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(int ScheduledCount, int SentCount)> GetStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        var scheduledCount = await context.Reminders
            .CountAsync(r => r.Status == ReminderStatus.Scheduled, cancellationToken);

        var sentCount = await context.Reminders
            .CountAsync(r => r.Status == ReminderStatus.Sent, cancellationToken);

        return (scheduledCount, sentCount);
    }

    public async Task<IReadOnlyList<Reminder>> GetDueRemindersAsync(
        DateTimeOffset asOf,
        CancellationToken cancellationToken = default)
    {
        return await context.Reminders
            .Where(r => r.Status == ReminderStatus.Scheduled && r.SendAt <= asOf)
            .OrderBy(r => r.SendAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        await context.Reminders.AddAsync(reminder, cancellationToken);
    }

    public async Task<bool> UpdateAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        var existing = await context.Reminders
            .FirstOrDefaultAsync(
                r => r.Id == reminder.Id && r.Status == ReminderStatus.Scheduled,
                cancellationToken);

        if (existing is null)
        {
            return false;
        }

        existing.Message = reminder.Message;
        existing.SendAt = reminder.SendAt;
        existing.Email = reminder.Email;

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var existing = await context.Reminders
            .FirstOrDefaultAsync(
                r => r.Id == id && r.Status == ReminderStatus.Scheduled,
                cancellationToken);

        if (existing is null)
        {
            return false;
        }

        context.Reminders.Remove(existing);

        return true;
    }

    public async Task<bool> MarkAsSentAsync(
        Guid id,
        DateTimeOffset sentAt,
        CancellationToken cancellationToken = default)
    {
        var reminder = await context.Reminders
            .FirstOrDefaultAsync(
                r => r.Id == id && r.Status == ReminderStatus.Scheduled,
                cancellationToken);

        if (reminder is null)
        {
            return false;
        }

        reminder.Status = ReminderStatus.Sent;
        reminder.SentAt = sentAt;
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
