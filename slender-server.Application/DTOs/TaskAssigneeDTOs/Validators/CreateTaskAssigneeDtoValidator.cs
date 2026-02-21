using FluentValidation;

namespace slender_server.Application.DTOs.TaskAssigneeDTOs.Validators;

public sealed class CreateTaskAssigneeDtoValidator : AbstractValidator<CreateTaskAssigneeDto>
{
    public CreateTaskAssigneeDtoValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.UserId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.AssignedById)
            .MaximumLength(50)
            .When(x => x.AssignedById is not null);
    }
}
