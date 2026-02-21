using FluentValidation;

namespace slender_server.Application.DTOs.TaskCommentAttachmentDTOs.Validators;

public sealed class UpdateTaskCommentAttachmentDtoValidator : AbstractValidator<UpdateTaskCommentAttachmentDto>
{
    public UpdateTaskCommentAttachmentDtoValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(500)
            .When(x => x.FileName is not null);

        RuleFor(x => x.FileUrl)
            .NotEmpty()
            .MaximumLength(2000)
            .When(x => x.FileUrl is not null);

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .When(x => x.FileSizeBytes is not null)
            .WithMessage("FileSizeBytes must be greater than 0");

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .MaximumLength(255)
            .When(x => x.MimeType is not null);
    }
}
