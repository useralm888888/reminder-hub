using Api.Data;
using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Dtos;
using Api.Exceptions;
using Api.Repositories;

namespace Api.Services;

public class ReminderService(
    IReminderRepository reminderRepository,
    IReminderDeliveryService deliveryService,
    IUnitOfWork unitOfWork,
    IReminderNotifier reminderNotifier,
    ILogger<ReminderService> logger) : IReminderService
{
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 50;

    public async Task<CreateReminderResponse> CreateAsync(
        CreateReminderRequest request,
        CancellationToken cancellationToken = default)
    {
        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            Message = request.Message.Trim(),
            SendAt = request.SendAt.ToUniversalTime(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Status = ReminderStatus.Scheduled,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await reminderRepository.AddAsync(reminder, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Reminder {ReminderId} scheduled for {SendAt}",
            reminder.Id,
            reminder.SendAt);

        return new CreateReminderResponse(reminder.Id, reminder.Status, reminder.SendAt);
    }

    public async Task<ReminderListResponse> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = Math.Max(1, page);
        var normalizedPageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var (items, totalCount) = await reminderRepository.GetPagedAsync(
            normalizedPage,
            normalizedPageSize,
            cancellationToken);

        var (scheduledCount, sentCount) = await reminderRepository.GetStatusCountsAsync(cancellationToken);

        return new ReminderListResponse(
            items.Select(MapToDto).ToList(),
            normalizedPage,
            normalizedPageSize,
            totalCount,
            scheduledCount,
            sentCount);
    }

    public async Task<ReminderDto> UpdateAsync(
        Guid id,
        UpdateReminderRequest request,
        CancellationToken cancellationToken = default)
    {
        var reminder = await reminderRepository.GetByIdAsync(id, cancellationToken);
        if (reminder is null)
        {
            throw new ReminderNotFoundException(id);
        }

        if (reminder.Status != ReminderStatus.Scheduled)
        {
            throw new ReminderNotEditableException(id);
        }

        reminder.Message = request.Message.Trim();
        reminder.SendAt = request.SendAt.ToUniversalTime();
        reminder.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();

        var updated = await reminderRepository.UpdateAsync(reminder, cancellationToken);
        if (!updated)
        {
            throw new ReminderNotEditableException(id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Reminder {ReminderId} updated", id);

        return MapToDto(reminder);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await reminderRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new ReminderNotFoundException(id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Reminder {ReminderId} deleted", id);
    }

    public async Task ProcessDueRemindersAsync(CancellationToken cancellationToken = default)
    {
        var dueReminders = await reminderRepository.GetDueRemindersAsync(
            DateTimeOffset.UtcNow,
            cancellationToken);

        if (dueReminders.Count == 0)
        {
            return;
        }

        logger.LogInformation("Processing {Count} due reminder(s)", dueReminders.Count);

        foreach (var reminder in dueReminders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await deliveryService.DeliverAsync(reminder, cancellationToken);

                var marked = await reminderRepository.TryMarkAsSentAsync(
                    reminder.Id,
                    DateTimeOffset.UtcNow,
                    cancellationToken);

                if (!marked)
                {
                    logger.LogWarning(
                        "Reminder {ReminderId} was already marked sent by another worker.",
                        reminder.Id);
                    continue;
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);

                logger.LogInformation(
                    "Reminder {ReminderId} marked as sent at {SentAt}",
                    reminder.Id,
                    DateTimeOffset.UtcNow);

                await reminderNotifier.NotifyStatusChangedAsync(
                    reminder.Id,
                    ReminderStatus.Sent,
                    cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(
                    ex,
                    "Failed to process reminder {ReminderId}. Will retry on next cycle.",
                    reminder.Id);
            }
        }
    }

    private static ReminderDto MapToDto(Reminder reminder)
    {
        return new ReminderDto(
            reminder.Id,
            reminder.Message,
            reminder.SendAt,
            reminder.Status,
            reminder.Email);
    }
}
