using FluentValidation;

namespace slender_server.Application.DTOs.MessageDTOs.Validators;

public sealed class CreateMessageDtoValidator : AbstractValidator<CreateMessageDto>
{
    public CreateMessageDtoValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ReplyToId)
            .MaximumLength(50)
            .When(x => x.ReplyToId is not null);

        RuleFor(x => x.Body)
            .NotEmpty()
            .Must(body => !string.IsNullOrWhiteSpace(body))
            .WithMessage("Body cannot be empty or whitespace");
    }
}
