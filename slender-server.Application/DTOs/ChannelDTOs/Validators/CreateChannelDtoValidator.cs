using FluentValidation;
using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ChannelDTOs.Validators;

public sealed class CreateChannelDtoValidator : AbstractValidator<CreateChannelDto>
{
    public CreateChannelDtoValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.CreatedById)
            .MaximumLength(50)
            .When(x => x.CreatedById is not null);

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(80)
            .When(x => x.Type != ChannelType.DirectMessage)
            .WithMessage("Name is required for Public and Private channels");

        RuleFor(x => x.Name)
            .Empty()
            .When(x => x.Type == ChannelType.DirectMessage)
            .WithMessage("Name must be null for DirectMessage channels");

        RuleFor(x => x.ProjectId)
            .MaximumLength(50)
            .When(x => x.ProjectId is not null);

        RuleFor(x => x.ProjectId)
            .Empty()
            .When(x => x.Type == ChannelType.DirectMessage)
            .WithMessage("ProjectId must be null for DirectMessage channels");

        RuleFor(x => x.ParticipantHash)
            .NotEmpty()
            .MaximumLength(64)
            .When(x => x.Type == ChannelType.DirectMessage)
            .WithMessage("ParticipantHash is required for DirectMessage channels");

        RuleFor(x => x.ParticipantHash)
            .Empty()
            .When(x => x.Type != ChannelType.DirectMessage)
            .WithMessage("ParticipantHash must be null for Public and Private channels");
    }
}
