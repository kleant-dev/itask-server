using FluentValidation;

namespace slender_server.Application.DTOs.WorkspaceMemberDTOs.Validators;

public sealed class CreateWorkspaceMemberDtoValidator : AbstractValidator<CreateWorkspaceMemberDto>
{
    public CreateWorkspaceMemberDtoValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Role)
            .IsInEnum();

        RuleFor(x => x.InvitedByUserId)
            .MaximumLength(50)
            .When(x => x.InvitedByUserId is not null);
    }
}
