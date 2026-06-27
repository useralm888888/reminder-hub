using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Dtos;
using FluentAssertions;

namespace Api.IntegrationTests;

public class RemindersEndpointsTests : IClassFixture<ReminderApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ReminderApiFactory _factory;

    public RemindersEndpointsTests(ReminderApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetReminders_ReturnsPagedResponseWithDefaultPageSize()
    {
        await CreateReminderAsync("First reminder");

        using var client = CreateAuthorizedClient();
        var response = await client.GetAsync("/reminders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<ReminderListResponse>(JsonOptions);
        payload.Should().NotBeNull();
        payload!.PageSize.Should().Be(50);
        payload.Items.Should().NotBeEmpty();
        payload.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateReminder_WithoutToken_ReturnsUnauthorized()
    {
        var created = await CreateReminderAsync("Update me");
        var client = _factory.CreateClient();

        var updateRequest = new UpdateReminderRequest(
            "Updated message",
            DateTimeOffset.UtcNow.AddHours(2),
            "updated@example.com");

        var response = await client.PutAsJsonAsync($"/reminders/{created.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateAndDeleteReminder_WithToken_Succeeds()
    {
        var created = await CreateReminderAsync("Mutable reminder");
        var authorizedClient = CreateAuthorizedClient();

        var updateRequest = new UpdateReminderRequest(
            "Updated message",
            DateTimeOffset.UtcNow.AddHours(3),
            "updated@example.com");

        var updateResponse = await authorizedClient.PutAsJsonAsync(
            $"/reminders/{created.Id}",
            updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ReminderDto>(JsonOptions);
        updated!.Message.Should().Be("Updated message");

        var deleteResponse = await authorizedClient.DeleteAsync($"/reminders/{created.Id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await authorizedClient.GetFromJsonAsync<ReminderListResponse>("/reminders", JsonOptions);
        listResponse!.Items.Should().NotContain(r => r.Id == created.Id);
    }

    private HttpClient CreateAuthorizedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        return client;
    }

    private async Task<CreateReminderResponse> CreateReminderAsync(string message)
    {
        using var authorizedClient = CreateAuthorizedClient();

        var request = new CreateReminderRequest(
            message,
            DateTimeOffset.UtcNow.AddHours(1),
            "recipient@example.com");

        var response = await authorizedClient.PostAsJsonAsync("/reminders", request);
        response.EnsureSuccessStatusCode();

        var created = await response.Content.ReadFromJsonAsync<CreateReminderResponse>(JsonOptions);
        return created!;
    }
}
