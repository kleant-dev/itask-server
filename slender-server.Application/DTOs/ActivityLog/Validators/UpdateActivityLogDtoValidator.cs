using FluentValidation;

namespace slender_server.Application.DTOs.ActivityLog.Validators;

public sealed class UpdateActivityLogDtoValidator : AbstractValidator<UpdateActivityLogDto>
{
    public UpdateActivityLogDtoValidator()
    {
        // OldValue and NewValue are JSON strings, no specific format validation
    }
}
