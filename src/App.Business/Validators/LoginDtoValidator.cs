using App.Domain.DTOs;
using FluentValidation;

namespace App.Business.Validators;

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(dto => dto.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(dto => dto.Password)
            .NotEmpty();
    }
}
