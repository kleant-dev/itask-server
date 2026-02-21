using FluentValidation;

namespace slender_server.Application.DTOs.ChannelDTOs.Validators;

public sealed class UpdateChannelDtoValidator : AbstractValidator<UpdateChannelDto>
{
    public UpdateChannelDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(80)
            .When(x => x.Name is not null);

        RuleFor(x => x.ProjectId)
            .MaximumLength(50)
            .When(x => x.ProjectId is not null);
    }
}
