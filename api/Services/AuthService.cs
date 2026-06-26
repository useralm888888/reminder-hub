using System.Security.Cryptography;
using System.Text;
using Api.Dtos;
using Api.Options;
using Microsoft.Extensions.Options;

namespace Api.Services;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest request);
}

public sealed class AuthService(
    IOptions<AuthOptions> authOptions,
    IOptions<ApiAuthOptions> apiAuthOptions) : IAuthService
{
    public LoginResponse? Login(LoginRequest request)
    {
        var auth = authOptions.Value;
        var scheduleToken = apiAuthOptions.Value.ScheduleToken;

        if (string.IsNullOrWhiteSpace(scheduleToken))
        {
            return null;
        }

        if (!CredentialsMatch(request.Username, auth.Username)
            || !CredentialsMatch(request.Password, auth.Password))
        {
            return null;
        }

        return new LoginResponse(scheduleToken, auth.Username);
    }

    private static bool CredentialsMatch(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return providedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}
