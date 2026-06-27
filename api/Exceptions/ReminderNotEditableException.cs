namespace Api.Exceptions;

public sealed class ReminderNotEditableException(Guid id)
    : Exception($"Reminder '{id}' cannot be edited because it has already been sent.")
{
    public Guid Id { get; } = id;
}
