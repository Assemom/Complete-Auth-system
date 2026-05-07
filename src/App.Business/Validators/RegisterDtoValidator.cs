using App.Domain.DTOs;
using App.Shared.Constants;
using FluentValidation;

namespace App.Business.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(dto => dto.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(dto => dto.FirstName)
            .NotEmpty();

        RuleFor(dto => dto.LastName)
            .NotEmpty();

        RuleFor(dto => dto.Password)
            .NotEmpty()
            .MinimumLength(ValidationConstants.PasswordMinLength)
            .Matches(ValidationConstants.PasswordUppercaseRegex)
            .Matches(ValidationConstants.PasswordLowercaseRegex)
            .Matches(ValidationConstants.PasswordDigitRegex)
            .Matches(ValidationConstants.PasswordSpecialRegex);

        RuleFor(dto => dto.ConfirmPassword)
            .Equal(dto => dto.Password);
    }
}
