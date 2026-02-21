using FluentValidation;

namespace slender_server.Application.DTOs.TaskAssigneeDTOs.Validators;

/// <summary>
/// TaskAssignee is a join entity with no updatable fields.
/// Validator provided for consistency but will always pass.
/// </summary>
public sealed class UpdateTaskAssigneeDtoValidator : AbstractValidator<UpdateTaskAssigneeDto>
{
    public UpdateTaskAssigneeDtoValidator()
    {
        // No validation rules - join entity with no updatable fields
    }
}
