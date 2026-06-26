using Api.Domain.Enums;

namespace Api.Dtos;

public record CreateReminderResponse(
    Guid Id,
    ReminderStatus Status,
    DateTimeOffset SendAt);
