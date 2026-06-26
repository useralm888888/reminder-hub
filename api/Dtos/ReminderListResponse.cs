namespace Api.Dtos;

public record ReminderListResponse(
    IReadOnlyList<ReminderDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int ScheduledCount,
    int SentCount);
