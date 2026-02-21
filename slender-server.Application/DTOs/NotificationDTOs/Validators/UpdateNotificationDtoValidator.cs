using FluentValidation;

namespace slender_server.Application.DTOs.NotificationDTOs.Validators;

public sealed class UpdateNotificationDtoValidator : AbstractValidator<UpdateNotificationDto>
{
    public UpdateNotificationDtoValidator()
    {
        // ReadAtUtc can be any DateTime value, no specific validation needed
    }
}
