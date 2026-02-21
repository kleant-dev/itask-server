using FluentValidation;

namespace slender_server.Application.DTOs.MessageDTOs.Validators;

public sealed class UpdateMessageDtoValidator : AbstractValidator<UpdateMessageDto>
{
    public UpdateMessageDtoValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty()
            .Must(body => !string.IsNullOrWhiteSpace(body))
            .When(x => x.Body is not null)
            .WithMessage("Body cannot be empty or whitespace");
    }
}
