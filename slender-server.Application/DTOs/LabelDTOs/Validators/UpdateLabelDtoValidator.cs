using FluentValidation;

namespace slender_server.Application.DTOs.LabelDTOs.Validators;

public sealed class UpdateLabelDtoValidator : AbstractValidator<UpdateLabelDto>
{
    public UpdateLabelDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50)
            .When(x => x.Name is not null);

        RuleFor(x => x.Color)
            .NotEmpty()
            .MaximumLength(7)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => x.Color is not null)
            .WithMessage("Color must be a valid hex color (e.g., #FF5733)");
    }
}
