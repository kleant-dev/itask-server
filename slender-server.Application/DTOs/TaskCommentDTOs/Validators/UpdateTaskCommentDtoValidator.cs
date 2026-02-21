using FluentValidation;

namespace slender_server.Application.DTOs.TaskCommentDTOs.Validators;

public sealed class UpdateTaskCommentDtoValidator : AbstractValidator<UpdateTaskCommentDto>
{
    public UpdateTaskCommentDtoValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty()
            .Must(body => !string.IsNullOrWhiteSpace(body))
            .When(x => x.Body is not null)
            .WithMessage("Body cannot be empty or whitespace");
    }
}
