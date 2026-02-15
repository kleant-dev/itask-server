using FluentValidation;

namespace itask_server.Application.DTOs.Auth.Validators;

public sealed class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();
    }
}