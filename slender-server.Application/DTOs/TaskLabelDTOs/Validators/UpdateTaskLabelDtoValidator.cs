using FluentValidation;

namespace slender_server.Application.DTOs.TaskLabelDTOs.Validators;

/// <summary>
/// TaskLabel is a join entity with no updatable fields.
/// Validator provided for consistency but will always pass.
/// </summary>
public sealed class UpdateTaskLabelDtoValidator : AbstractValidator<UpdateTaskLabelDto>
{
    public UpdateTaskLabelDtoValidator()
    {
        // No validation rules - join entity with no updatable fields
    }
}
