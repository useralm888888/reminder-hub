using System.Text;
using Api.Domain.Entities;
using Api.Options;
using Microsoft.Extensions.Options;

namespace Api.Services;

public class FileReminderDeliveryService : IReminderDeliveryService
{
    private readonly ReminderDeliveryOptions _options;
    private readonly ILogger<FileReminderDeliveryService> _logger;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public FileReminderDeliveryService(
        IOptions<ReminderDeliveryOptions> options,
        ILogger<FileReminderDeliveryService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task DeliverAsync(Reminder reminder, CancellationToken cancellationToken = default)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss'Z'");
        var logLine = $"[{timestamp}] Reminder sent: {reminder.Message}";

        _logger.LogInformation(
            "Reminder {ReminderId} delivered: {Message}",
            reminder.Id,
            reminder.Message);

        var logDirectory = Path.GetDirectoryName(_options.LogFilePath);
        if (!string.IsNullOrWhiteSpace(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await File.AppendAllTextAsync(
                _options.LogFilePath,
                logLine + Environment.NewLine,
                Encoding.UTF8,
                cancellationToken);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}
