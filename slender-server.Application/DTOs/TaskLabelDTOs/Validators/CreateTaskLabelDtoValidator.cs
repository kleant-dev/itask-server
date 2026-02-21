using FluentValidation;

namespace slender_server.Application.DTOs.TaskLabelDTOs.Validators;

public sealed class CreateTaskLabelDtoValidator : AbstractValidator<CreateTaskLabelDto>
{
    public CreateTaskLabelDtoValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LabelId)
            .NotEmpty()
            .MaximumLength(50);
    }
}
