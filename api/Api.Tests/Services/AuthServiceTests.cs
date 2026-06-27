using Api.Dtos;
using Api.Options;
using Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Api.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public void Login_WhenCredentialsMatch_ReturnsScheduleToken()
    {
        var sut = CreateService("admin", "admin", "schedule-token");

        var response = sut.Login(new LoginRequest("admin", "admin"));

        response.Should().NotBeNull();
        response!.Token.Should().Be("schedule-token");
        response.Username.Should().Be("admin");
    }

    [Fact]
    public void Login_WhenPasswordIsWrong_ReturnsNull()
    {
        var sut = CreateService("admin", "admin", "schedule-token");

        var response = sut.Login(new LoginRequest("admin", "wrong"));

        response.Should().BeNull();
    }

    [Fact]
    public void Login_WhenScheduleTokenMissing_ReturnsNull()
    {
        var sut = CreateService("admin", "admin", scheduleToken: " ");

        var response = sut.Login(new LoginRequest("admin", "admin"));

        response.Should().BeNull();
    }

    private static AuthService CreateService(string username, string password, string scheduleToken)
    {
        return new AuthService(
            Microsoft.Extensions.Options.Options.Create(new AuthOptions
            {
                Username = username,
                Password = password,
            }),
            Microsoft.Extensions.Options.Options.Create(new ApiAuthOptions
            {
                ScheduleToken = scheduleToken,
            }));
    }
}
