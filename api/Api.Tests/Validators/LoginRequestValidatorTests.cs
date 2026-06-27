using Api.Dtos;
using Api.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Api.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public void Validate_WhenCredentialsMissing_HasValidationErrors()
    {
        var result = _sut.TestValidate(new LoginRequest(string.Empty, string.Empty));

        result.ShouldHaveValidationErrorFor(x => x.Username);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WhenCredentialsPresent_IsValid()
    {
        var result = _sut.TestValidate(new LoginRequest("admin", "admin"));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
