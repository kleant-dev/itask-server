using FluentValidation;

namespace slender_server.Application.DTOs.WorkspaceInviteDTOs.Validators;

public sealed class UpdateWorkspaceInviteDtoValidator : AbstractValidator<UpdateWorkspaceInviteDto>
{
    public UpdateWorkspaceInviteDtoValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum()
            .When(x => x.Role is not null);

        RuleFor(x => x.ExpiresAtUtc)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ExpiresAtUtc is not null)
            .WithMessage("ExpiresAtUtc must be in the future");
    }
}
