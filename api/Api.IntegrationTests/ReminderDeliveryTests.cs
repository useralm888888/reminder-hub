using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Data;
using Api.Domain.Enums;
using Api.Dtos;
using Api.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests;

public class ReminderDeliveryTests : IClassFixture<ReminderApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ReminderApiFactory _factory;

    public ReminderDeliveryTests(ReminderApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProcessDueRemindersAsync_MarksDueReminderAsSent()
    {
        var created = await CreateReminderAsync("Deliver on schedule");

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var entity = await db.Reminders.FindAsync(created.Id);
        entity.Should().NotBeNull();
        entity!.SendAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
        await reminderService.ProcessDueRemindersAsync();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        var list = await client.GetFromJsonAsync<ReminderListResponse>("/reminders", JsonOptions);
        var item = list!.Items.Single(r => r.Id == created.Id);
        item.Status.Should().Be(ReminderStatus.Sent);
    }

    private async Task<CreateReminderResponse> CreateReminderAsync(string message)
    {
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

        var request = new CreateReminderRequest(
            message,
            DateTimeOffset.UtcNow.AddHours(1),
            "recipient@example.com");

        var response = await client.PostAsJsonAsync("/reminders", request);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateReminderResponse>(JsonOptions);
        return created!;
    }
}
