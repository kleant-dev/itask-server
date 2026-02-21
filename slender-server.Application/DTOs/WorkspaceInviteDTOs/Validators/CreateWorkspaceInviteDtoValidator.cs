using FluentValidation;

namespace slender_server.Application.DTOs.WorkspaceInviteDTOs.Validators;

public sealed class CreateWorkspaceInviteDtoValidator : AbstractValidator<CreateWorkspaceInviteDto>
{
    public CreateWorkspaceInviteDtoValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.InvitedByUserId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.Role)
            .IsInEnum();

        RuleFor(x => x.ExpiresAtUtc)
            .NotEmpty()
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("ExpiresAtUtc must be in the future");
    }
}
