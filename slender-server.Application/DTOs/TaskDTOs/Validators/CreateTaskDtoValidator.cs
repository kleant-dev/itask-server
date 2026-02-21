using FluentValidation;

namespace slender_server.Application.DTOs.TaskDTOs.Validators;

public sealed class CreateTaskDtoValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.CreatedById)
            .MaximumLength(50)
            .When(x => x.CreatedById is not null);

        RuleFor(x => x.ParentTaskId)
            .MaximumLength(50)
            .When(x => x.ParentTaskId is not null);

        RuleFor(x => x.Status).IsInEnum();

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);
        
        RuleFor(x => x.Priority)
            .IsInEnum();

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes is not null)
            .WithMessage("DurationMinutes must be greater than 0");
    }
}
