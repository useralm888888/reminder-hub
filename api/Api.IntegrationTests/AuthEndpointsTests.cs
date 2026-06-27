using System.Net;
using System.Net.Http.Json;
using Api.Dtos;
using FluentAssertions;

namespace Api.IntegrationTests;

public class AuthEndpointsTests : IClassFixture<ReminderApiFactory>
{
    private readonly ReminderApiFactory _factory;

    public AuthEndpointsTests(ReminderApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest("admin", "admin"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();
        payload.Should().NotBeNull();
        payload!.Token.Should().Be("test-token");
        payload.Username.Should().Be("admin");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest("admin", "wrong-password"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyCredentials_ReturnsBadRequest()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest(string.Empty, string.Empty));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
