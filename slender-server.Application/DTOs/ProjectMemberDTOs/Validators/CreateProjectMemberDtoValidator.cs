using FluentValidation;

namespace slender_server.Application.DTOs.ProjectMemberDTOs.Validators;

public sealed class CreateProjectMemberDtoValidator : AbstractValidator<CreateProjectMemberDto>
{
    public CreateProjectMemberDtoValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Role)
            .IsInEnum();

        RuleFor(x => x.AddedByUserId)
            .MaximumLength(50)
            .When(x => x.AddedByUserId is not null);
    }
}
