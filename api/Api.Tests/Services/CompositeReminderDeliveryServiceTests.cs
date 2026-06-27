using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Options;
using Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Api.Tests.Services;

public class CompositeReminderDeliveryServiceTests : IDisposable
{
    private readonly string _logPath;
    private readonly Mock<IReminderEmailSender> _emailSender = new();
    private readonly CompositeReminderDeliveryService _sut;

    public CompositeReminderDeliveryServiceTests()
    {
        _logPath = Path.Combine(Path.GetTempPath(), $"reminder-test-{Guid.NewGuid()}.log");
        var fileDelivery = new FileReminderDeliveryService(
            Microsoft.Extensions.Options.Options.Create(new ReminderDeliveryOptions { LogFilePath = _logPath }),
            Mock.Of<ILogger<FileReminderDeliveryService>>());

        _sut = new CompositeReminderDeliveryService(
            fileDelivery,
            _emailSender.Object,
            Mock.Of<ILogger<CompositeReminderDeliveryService>>());
    }

    [Fact]
    public async Task DeliverAsync_WhenEmailFails_StillCompletesAfterFileDelivery()
    {
        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            Message = "Test",
            SendAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            Email = "user@example.com",
            Status = ReminderStatus.Scheduled,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _emailSender
            .Setup(s => s.SendAsync(reminder, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP rejected"));

        var act = () => _sut.DeliverAsync(reminder);

        await act.Should().NotThrowAsync();
        File.Exists(_logPath).Should().BeTrue();
        (await File.ReadAllTextAsync(_logPath)).Should().Contain("Reminder sent: Test");
    }

    public void Dispose()
    {
        if (File.Exists(_logPath))
        {
            File.Delete(_logPath);
        }
    }
}
