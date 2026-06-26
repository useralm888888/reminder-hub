using Api.Domain.Enums;

namespace Api.Domain.Entities;

public class Reminder
{
    public Guid Id { get; set; }

    public required string Message { get; set; }

    public DateTimeOffset SendAt { get; set; }

    public string? Email { get; set; }

    public ReminderStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? SentAt { get; set; }
}
