using Api.Dtos;
using Api.Validators;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators;

public class CreateReminderRequestValidatorTests
{
    private readonly CreateReminderRequestValidator _sut = new();

    [Fact]
    public void Validate_WhenSendAtIsInPast_HasValidationError()
    {
        var result = _sut.TestValidate(new CreateReminderRequest(
            "Message",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            null));

        result.ShouldHaveValidationErrorFor(x => x.SendAt);
    }

    [Fact]
    public void Validate_WhenMessageEmpty_HasValidationError()
    {
        var result = _sut.TestValidate(new CreateReminderRequest(
            string.Empty,
            DateTimeOffset.UtcNow.AddHours(1),
            null));

        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Validate_WhenRequestIsValid_HasNoErrors()
    {
        var result = _sut.TestValidate(new CreateReminderRequest(
            "Reminder",
            DateTimeOffset.UtcNow.AddHours(1),
            "user@example.com"));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
