using FluentValidation;

namespace slender_server.Application.DTOs.ChannelMemberDTOs.Validators;

public sealed class CreateChannelMemberDtoValidator : AbstractValidator<CreateChannelMemberDto>
{
    public CreateChannelMemberDtoValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.UserId)
            .MaximumLength(50)
            .When(x => x.UserId is not null);
    }
}
