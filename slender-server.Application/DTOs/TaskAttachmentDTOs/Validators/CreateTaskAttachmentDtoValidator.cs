using FluentValidation;

namespace slender_server.Application.DTOs.TaskAttachmentDTOs.Validators;

public sealed class CreateTaskAttachmentDtoValidator : AbstractValidator<CreateTaskAttachmentDto>
{
    public CreateTaskAttachmentDtoValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.UploadedById)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.FileName)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.FileUrl)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("FileSizeBytes must be greater than 0");

        RuleFor(x => x.MimeType)
            .NotEmpty()
            .MaximumLength(255);
    }
}
