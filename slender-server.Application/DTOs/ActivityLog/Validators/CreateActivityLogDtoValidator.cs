using FluentValidation;

namespace slender_server.Application.DTOs.ActivityLog.Validators;

public sealed class CreateActivityLogDtoValidator : AbstractValidator<CreateActivityLogDto>
{
    public CreateActivityLogDtoValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ActorId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Action)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.EntityType)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .MaximumLength(50);
    }
}
