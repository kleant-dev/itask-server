namespace slender_server.Application.DTOs.TaskAssigneeDTOs;
using Domain.Entities;

public sealed record TaskAssigneeDto
{
    public required string TaskId { get; init; }
    public required string UserId { get; init; }
    public string? AssignedById { get; init; }
    public DateTime AssignedAtUtc { get; init; }
}

public static class TaskAssigneeDtoExtensions
{
    public static TaskAssigneeDto ToDto(this TaskAssignee entity)
    {
        return new TaskAssigneeDto
        {
            TaskId = entity.TaskId,
            UserId = entity.UserId,
            AssignedById = entity.AssignedById,
            AssignedAtUtc = entity.AssignedAtUtc
        };
    }
}
