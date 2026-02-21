using FluentValidation;

namespace slender_server.Application.DTOs.NotificationDTOs.Validators;

public sealed class CreateNotificationDtoValidator : AbstractValidator<CreateNotificationDto>
{
    public CreateNotificationDtoValidator()
    {
        RuleFor(x => x.RecipientId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ActorId)
            .MaximumLength(50)
            .When(x => x.ActorId is not null);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.WorkspaceId)
            .MaximumLength(50)
            .When(x => x.WorkspaceId is not null);

        RuleFor(x => x.ProjectId)
            .MaximumLength(50)
            .When(x => x.ProjectId is not null);

        RuleFor(x => x.TaskId)
            .MaximumLength(50)
            .When(x => x.TaskId is not null);

        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.EntityName)
            .MaximumLength(500)
            .When(x => x.EntityName is not null);

        RuleFor(x => x)
            .Must(n => new[] { n.WorkspaceId, n.ProjectId, n.TaskId }.Count(id => id is not null) <= 1)
            .WithMessage("Only one of WorkspaceId, ProjectId, or TaskId can be set");
    }
}
