using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Dtos;
using Api.Exceptions;
using Api.Repositories;
using Api.Services;
using Api.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Api.Tests.Services;

public class ReminderServiceTests
{
    private readonly Mock<IReminderRepository> _repository = new();
    private readonly Mock<IReminderDeliveryService> _deliveryService = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IReminderNotifier> _notifier = new();
    private readonly ReminderService _sut;

    public ReminderServiceTests()
    {
        _sut = new ReminderService(
            _repository.Object,
            _deliveryService.Object,
            _unitOfWork.Object,
            _notifier.Object,
            Mock.Of<ILogger<ReminderService>>());
    }

    [Fact]
    public async Task GetPagedAsync_ClampsPageSizeToMaximum()
    {
        _repository
            .Setup(r => r.GetPagedAsync(1, ReminderService.MaxPageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Array.Empty<Reminder>(), 0));

        _repository
            .Setup(r => r.GetStatusCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0));

        var result = await _sut.GetPagedAsync(1, 500);

        result.PageSize.Should().Be(ReminderService.MaxPageSize);
        _repository.Verify(r => r.GetPagedAsync(1, ReminderService.MaxPageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenReminderNotFound_ThrowsReminderNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reminder?)null);

        var act = () => _sut.UpdateAsync(
            id,
            new UpdateReminderRequest("Updated", DateTimeOffset.UtcNow.AddHours(1), null));

        await act.Should().ThrowAsync<ReminderNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenReminderAlreadySent_ThrowsReminderNotEditableException()
    {
        var reminder = CreateReminder(status: ReminderStatus.Sent);
        _repository
            .Setup(r => r.GetByIdAsync(reminder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reminder);

        var act = () => _sut.UpdateAsync(
            reminder.Id,
            new UpdateReminderRequest("Updated", DateTimeOffset.UtcNow.AddHours(1), null));

        await act.Should().ThrowAsync<ReminderNotEditableException>();
    }

    [Fact]
    public async Task CreateAsync_PersistsReminderAndReturnsResponse()
    {
        Reminder? captured = null;
        _repository
            .Setup(r => r.AddAsync(It.IsAny<Reminder>(), It.IsAny<CancellationToken>()))
            .Callback<Reminder, CancellationToken>((reminder, _) => captured = reminder)
            .Returns(Task.CompletedTask);

        var sendAt = DateTimeOffset.UtcNow.AddHours(2);
        var request = new CreateReminderRequest("  Check logs  ", sendAt, "  user@example.com  ");

        var response = await _sut.CreateAsync(request);

        response.Status.Should().Be(ReminderStatus.Scheduled);
        response.SendAt.Should().Be(sendAt.ToUniversalTime());
        captured.Should().NotBeNull();
        captured!.Message.Should().Be("Check logs");
        captured.Email.Should().Be("user@example.com");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDueRemindersAsync_WhenAlreadyMarkedSent_DoesNotNotify()
    {
        var reminder = CreateReminder();
        reminder.SendAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        _repository
            .Setup(r => r.GetDueRemindersAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _repository
            .Setup(r => r.TryMarkAsSentAsync(reminder.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.ProcessDueRemindersAsync();

        _notifier.Verify(
            n => n.NotifyStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<ReminderStatus>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDueRemindersAsync_WhenMarkedSent_NotifiesClients()
    {
        var reminder = CreateReminder();
        reminder.SendAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        _repository
            .Setup(r => r.GetDueRemindersAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _repository
            .Setup(r => r.TryMarkAsSentAsync(reminder.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.ProcessDueRemindersAsync();

        _notifier.Verify(
            n => n.NotifyStatusChangedAsync(reminder.Id, ReminderStatus.Sent, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenReminderMissing_ThrowsReminderNotFoundException()
    {
        var id = Guid.NewGuid();
        _repository
            .Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = () => _sut.DeleteAsync(id);

        await act.Should().ThrowAsync<ReminderNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_WhenReminderIsScheduled_DeletesReminder()
    {
        var reminder = CreateReminder();

        _repository
            .Setup(r => r.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.DeleteAsync(reminder.Id);

        _repository.Verify(r => r.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenReminderAlreadySent_DeletesReminder()
    {
        var reminder = CreateReminder(status: ReminderStatus.Sent);

        _repository
            .Setup(r => r.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.DeleteAsync(reminder.Id);

        _repository.Verify(r => r.DeleteAsync(reminder.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDueRemindersAsync_WhenNoDueReminders_DoesNotDeliver()
    {
        _repository
            .Setup(r => r.GetDueRemindersAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Reminder>());

        await _sut.ProcessDueRemindersAsync();

        _deliveryService.Verify(
            d => d.DeliverAsync(It.IsAny<Reminder>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDueRemindersAsync_WhenDueReminder_DeliversAndMarksAsSent()
    {
        var reminder = CreateReminder();
        reminder.SendAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        _repository
            .Setup(r => r.GetDueRemindersAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _repository
            .Setup(r => r.TryMarkAsSentAsync(reminder.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.ProcessDueRemindersAsync();

        _deliveryService.Verify(
            d => d.DeliverAsync(reminder, It.IsAny<CancellationToken>()),
            Times.Once);

        _repository.Verify(
            r => r.TryMarkAsSentAsync(reminder.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessDueRemindersAsync_WhenDeliveryFails_DoesNotMarkAsSent()
    {
        var reminder = CreateReminder();
        reminder.SendAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        _repository
            .Setup(r => r.GetDueRemindersAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reminder> { reminder });

        _deliveryService
            .Setup(d => d.DeliverAsync(reminder, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Log file is not writable."));

        await _sut.ProcessDueRemindersAsync();

        _repository.Verify(
            r => r.TryMarkAsSentAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Reminder CreateReminder(ReminderStatus status = ReminderStatus.Scheduled)
    {
        return new Reminder
        {
            Id = Guid.NewGuid(),
            Message = "Test",
            SendAt = DateTimeOffset.UtcNow.AddHours(2),
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
