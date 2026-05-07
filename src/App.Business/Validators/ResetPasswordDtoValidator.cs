using App.Domain.DTOs;
using App.Shared.Constants;
using FluentValidation;

namespace App.Business.Validators;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(dto => dto.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(dto => dto.Token)
            .NotEmpty();

        RuleFor(dto => dto.NewPassword)
            .NotEmpty()
            .MinimumLength(ValidationConstants.PasswordMinLength)
            .Matches(ValidationConstants.PasswordUppercaseRegex)
            .Matches(ValidationConstants.PasswordLowercaseRegex)
            .Matches(ValidationConstants.PasswordDigitRegex)
            .Matches(ValidationConstants.PasswordSpecialRegex);

        RuleFor(dto => dto.ConfirmNewPassword)
            .Equal(dto => dto.NewPassword);
    }
}
