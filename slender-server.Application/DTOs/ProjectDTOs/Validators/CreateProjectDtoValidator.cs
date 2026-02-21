using FluentValidation;

namespace slender_server.Application.DTOs.ProjectDTOs.Validators;

public sealed class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
{
    public CreateProjectDtoValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.Color)
            .MaximumLength(7)
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .When(x => x.Color is not null)
            .WithMessage("Color must be a valid hex color (e.g., #FF5733)");

        RuleFor(x => x.Icon)
            .MaximumLength(500)
            .When(x => x.Icon is not null);

        RuleFor(x => x.TargetDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.StartDate is not null && x.TargetDate is not null)
            .WithMessage("TargetDate must be after StartDate");
    }
}
