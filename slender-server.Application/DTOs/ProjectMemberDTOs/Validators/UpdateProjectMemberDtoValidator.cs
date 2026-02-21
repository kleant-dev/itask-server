using FluentValidation;

namespace slender_server.Application.DTOs.ProjectMemberDTOs.Validators;

public sealed class UpdateProjectMemberDtoValidator : AbstractValidator<UpdateProjectMemberDto>
{
    public UpdateProjectMemberDtoValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum()
            .When(x => x.Role is not null);
    }
}
