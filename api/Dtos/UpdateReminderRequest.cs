namespace Api.Dtos;

public record UpdateReminderRequest(
    string Message,
    DateTimeOffset SendAt,
    string? Email);
