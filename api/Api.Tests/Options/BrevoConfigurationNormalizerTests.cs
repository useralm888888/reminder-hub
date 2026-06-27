using Api.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Api.Tests.Options;

public class BrevoConfigurationNormalizerTests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("\"true\"", true)]
    [InlineData("false", false)]
    [InlineData("\"false\"", false)]
    public void Apply_ParsesEnabledWithOrWithoutQuotes(string enabledValue, bool expected)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Brevo:Enabled"] = enabledValue,
                ["Brevo:ApiKey"] = "xkeysib-test",
                ["Brevo:SenderEmail"] = "sender@example.com",
            })
            .Build();

        var options = new BrevoOptions();
        BrevoConfigurationNormalizer.Apply(options, configuration);

        options.Enabled.Should().Be(expected);
        options.ApiKey.Should().Be("xkeysib-test");
        options.IsConfigured.Should().Be(expected);
    }
}
