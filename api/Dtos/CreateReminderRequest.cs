namespace Api.Dtos;

public record CreateReminderRequest(
    string Message,
    DateTimeOffset SendAt,
    string? Email);
