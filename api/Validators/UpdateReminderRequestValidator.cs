using Api.Dtos;
using FluentValidation;

namespace Api.Validators;

public class UpdateReminderRequestValidator : AbstractValidator<UpdateReminderRequest>
{
    public UpdateReminderRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.SendAt)
            .Must(sendAt => sendAt > DateTimeOffset.UtcNow)
            .WithMessage("SendAt must be a future date and time in UTC.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .MaximumLength(320)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
