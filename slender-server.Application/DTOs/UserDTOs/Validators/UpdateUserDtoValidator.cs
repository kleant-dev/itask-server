using FluentValidation;

namespace slender_server.Application.DTOs.UserDTOs.Validators;

public sealed class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(255)
            .When(x => x.Email is not null);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Name is not null);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(500)
            .When(x => x.AvatarUrl is not null);

        RuleFor(x => x.DisplayName)
            .MaximumLength(100)
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.AvatarColor)
            .MaximumLength(7)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => x.AvatarColor is not null)
            .WithMessage("AvatarColor must be a valid hex color (e.g., #FF5733)");
    }
}
