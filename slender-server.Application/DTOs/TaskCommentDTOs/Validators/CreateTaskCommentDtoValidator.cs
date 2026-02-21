using FluentValidation;

namespace slender_server.Application.DTOs.TaskCommentDTOs.Validators;

public sealed class CreateTaskCommentDtoValidator : AbstractValidator<CreateTaskCommentDto>
{
    public CreateTaskCommentDtoValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.AuthorId)
            .MaximumLength(50)
            .When(x => x.AuthorId is not null);

        RuleFor(x => x.ParentCommentId)
            .MaximumLength(50)
            .When(x => x.ParentCommentId is not null);

        RuleFor(x => x.Body)
            .NotEmpty()
            .Must(body => !string.IsNullOrWhiteSpace(body))
            .WithMessage("Body cannot be empty or whitespace");
    }
}
