using App.Domain.DTOs;
using App.Shared.Constants;
using FluentValidation;

namespace App.Business.Validators;

public class ConfirmEmailDtoValidator : AbstractValidator<ConfirmEmailDto>
{
    public ConfirmEmailDtoValidator()
    {
        RuleFor(dto => dto.UserId)
            .NotEmpty();

        RuleFor(dto => dto.Code)
            .NotEmpty()
            .Matches(ValidationConstants.OtpRegex);
    }
}
