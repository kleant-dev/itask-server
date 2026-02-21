using FluentValidation;

namespace slender_server.Application.DTOs.TaskDTOs.Validators;

public sealed class UpdateTaskDtoValidator : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500)
            .When(x => x.Title is not null);

        RuleFor(x => x.Status)
            .IsInEnum()
            .When(x => x.Status is not null);

        RuleFor(x => x.Priority)
            .IsInEnum()
            .When(x => x.Priority is not null);

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .When(x => x.DurationMinutes is not null)
            .WithMessage("DurationMinutes must be greater than 0");
    }
}
