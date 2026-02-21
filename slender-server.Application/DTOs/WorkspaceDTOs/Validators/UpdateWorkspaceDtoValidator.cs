using FluentValidation;

namespace slender_server.Application.DTOs.WorkspaceDTOs.Validators;

public sealed class UpdateWorkspaceDtoValidator : AbstractValidator<UpdateWorkspaceDto>
{
    public UpdateWorkspaceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.Name is not null);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(60)
            .Matches(@"^[a-z0-9-]+$")
            .When(x => x.Slug is not null)
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");

        RuleFor(x => x.LogoUrl)
            .MaximumLength(500)
            .When(x => x.LogoUrl is not null);
    }
}
