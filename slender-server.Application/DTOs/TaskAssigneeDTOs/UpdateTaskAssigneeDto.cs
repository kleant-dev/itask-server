using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.TaskAssigneeDTOs;

/// <summary>
/// TaskAssignee is a join entity; typically only creation/deletion is used.
/// Update DTO provided for consistency; has no updatable fields.
/// </summary>
public sealed record UpdateTaskAssigneeDto;

public static class UpdateTaskAssigneeDtoExtensions
{
    public static void ApplyTo(this UpdateTaskAssigneeDto _, TaskAssignee __) { }
}
