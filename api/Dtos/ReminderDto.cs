using Api.Domain.Enums;

namespace Api.Dtos;

public record ReminderDto(
    Guid Id,
    string Message,
    DateTimeOffset SendAt,
    ReminderStatus Status,
    string? Email);
