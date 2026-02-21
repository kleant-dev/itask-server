using FluentValidation;

namespace slender_server.Application.DTOs.WorkspaceMemberDTOs.Validators;

public sealed class UpdateWorkspaceMemberDtoValidator : AbstractValidator<UpdateWorkspaceMemberDto>
{
    public UpdateWorkspaceMemberDtoValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum()
            .When(x => x.Role is not null);
    }
}
