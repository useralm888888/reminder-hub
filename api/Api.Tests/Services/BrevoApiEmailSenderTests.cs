using System.Net;
using System.Text.Json;
using Api.Domain.Entities;
using Api.Domain.Enums;
using Api.Options;
using Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Api.Tests.Services;

public class BrevoApiEmailSenderTests
{
    [Fact]
    public async Task SendAsync_WhenApiReturnsSuccess_CompletesWithoutError()
    {
        string? capturedBody = null;
        var handler = new StubHttpMessageHandler(async request =>
        {
            capturedBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.Created);
        });
        var sender = CreateSender(handler);

        var reminder = CreateReminder("user@example.com");

        await sender.SendAsync(reminder);

        handler.RequestCount.Should().Be(1);
        handler.LastRequest!.RequestUri!.AbsolutePath.Should().Be("/v3/smtp/email");
        handler.LastRequest.Headers.GetValues("api-key").Single().Should().Be("test-api-key");

        using var json = JsonDocument.Parse(capturedBody!);
        json.RootElement.TryGetProperty("sender", out _).Should().BeTrue();
        json.RootElement.TryGetProperty("textContent", out _).Should().BeTrue();
        json.RootElement.GetProperty("sender").GetProperty("email").GetString().Should().Be("sender@example.com");
    }

    [Fact]
    public async Task SendAsync_WhenApiReturnsError_Throws()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent("{\"message\":\"Invalid API key\"}"),
            }));
        var sender = CreateSender(handler);

        var reminder = CreateReminder("user@example.com");

        var act = () => sender.SendAsync(reminder);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*401*Invalid API key*");
    }

    [Fact]
    public async Task SendAsync_WhenNotConfigured_DoesNotCallApi()
    {
        var handler = new StubHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.Created)));
        var sender = new BrevoApiEmailSender(
            new StubHttpClientFactory(handler),
            Microsoft.Extensions.Options.Options.Create(new BrevoOptions { Enabled = false, ApiKey = "", SenderEmail = "sender@example.com" }),
            Mock.Of<ILogger<BrevoApiEmailSender>>());

        await sender.SendAsync(CreateReminder("user@example.com"));

        handler.RequestCount.Should().Be(0);
    }

    private static BrevoApiEmailSender CreateSender(StubHttpMessageHandler handler)
    {
        return new BrevoApiEmailSender(
            new StubHttpClientFactory(handler),
            Microsoft.Extensions.Options.Options.Create(new BrevoOptions
            {
                Enabled = true,
                ApiKey = "test-api-key",
                SenderEmail = "sender@example.com",
                SenderName = "Reminder Hub",
            }),
            Mock.Of<ILogger<BrevoApiEmailSender>>());
    }

    private static Reminder CreateReminder(string email)
    {
        return new Reminder
        {
            Id = Guid.NewGuid(),
            Message = "Test reminder",
            SendAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            Email = email,
            Status = ReminderStatus.Scheduled,
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    private sealed class StubHttpClientFactory(StubHttpMessageHandler handler) : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient(handler, disposeHandler: false)
            {
                BaseAddress = new Uri("https://api.brevo.com/"),
            };
        }
    }

    private sealed class StubHttpMessageHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequest = request;
            return await responder(request);
        }
    }
}
