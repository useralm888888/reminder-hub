using Api.Dtos;

namespace Api.Services;

public interface IReminderService
{
    Task<CreateReminderResponse> CreateAsync(
        CreateReminderRequest request,
        CancellationToken cancellationToken = default);

    Task<ReminderListResponse> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<ReminderDto> UpdateAsync(
        Guid id,
        UpdateReminderRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task ProcessDueRemindersAsync(CancellationToken cancellationToken = default);
}
