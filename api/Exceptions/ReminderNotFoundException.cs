namespace Api.Exceptions;

public sealed class ReminderNotFoundException(Guid id) : Exception($"Reminder '{id}' was not found.")
{
    public Guid Id { get; } = id;
}
