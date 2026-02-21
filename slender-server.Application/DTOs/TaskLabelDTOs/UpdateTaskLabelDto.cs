using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.TaskLabelDTOs;

/// <summary>
/// TaskLabel is a join entity with no updatable fields.
/// </summary>
public sealed record UpdateTaskLabelDto;

public static class UpdateTaskLabelDtoExtensions
{
    public static void ApplyTo(this UpdateTaskLabelDto _, TaskLabel __) { }
}
