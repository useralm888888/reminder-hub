using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Api.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Api.Authentication;

public sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<ApiAuthOptions> apiAuthOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var configuredToken = apiAuthOptions.Value.ScheduleToken;
        if (string.IsNullOrWhiteSpace(configuredToken))
        {
            return Task.FromResult(AuthenticateResult.Fail("API schedule token is not configured."));
        }

        if (!TryGetBearerToken(out var providedToken))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing Authorization header."));
        }

        if (!TokensMatch(providedToken, configuredToken))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API token."));
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, "schedule-client")],
            Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private bool TryGetBearerToken(out string token)
    {
        token = string.Empty;
        const string bearerPrefix = "Bearer ";

        if (Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            var headerValue = authorizationHeader.ToString();
            if (headerValue.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                token = headerValue[bearerPrefix.Length..].Trim();
                return token.Length > 0;
            }

            return false;
        }

        if (Request.Query.TryGetValue("access_token", out var accessToken))
        {
            token = accessToken.ToString().Trim();
            return token.Length > 0;
        }

        return false;
    }

    private static bool TokensMatch(string provided, string expected)
    {
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return providedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
    }
}
