using Api.Data;
using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Api.Tests.Repositories;

public class ReminderRepositoryTests
{
    [Fact]
    public async Task TryMarkAsSentAsync_WhenScheduled_UpdatesStatusAndVersion()
    {
        await using var context = CreateContext();
        var reminder = await SeedScheduledReminderAsync(context);
        var repository = new ReminderRepository(context);

        var marked = await repository.TryMarkAsSentAsync(reminder.Id, DateTimeOffset.UtcNow);

        marked.Should().BeTrue();
        await context.SaveChangesAsync();

        var updated = await context.Reminders.SingleAsync(r => r.Id == reminder.Id);
        updated.Status.Should().Be(ReminderStatus.Sent);
        updated.SentAt.Should().NotBeNull();
        updated.Version.Should().Be(1);
    }

    [Fact]
    public async Task TryMarkAsSentAsync_WhenAlreadySent_ReturnsFalse()
    {
        await using var context = CreateContext();
        var reminder = await SeedScheduledReminderAsync(context);
        reminder.Status = ReminderStatus.Sent;
        reminder.SentAt = DateTimeOffset.UtcNow;
        reminder.Version = 1;
        await context.SaveChangesAsync();

        var repository = new ReminderRepository(context);

        var marked = await repository.TryMarkAsSentAsync(reminder.Id, DateTimeOffset.UtcNow);

        marked.Should().BeFalse();
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<Reminder> SeedScheduledReminderAsync(AppDbContext context)
    {
        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            Message = "Repository test",
            SendAt = DateTimeOffset.UtcNow.AddHours(1),
            Status = ReminderStatus.Scheduled,
            CreatedAt = DateTimeOffset.UtcNow,
            Version = 0,
        };

        context.Reminders.Add(reminder);
        await context.SaveChangesAsync();

        return reminder;
    }
}
