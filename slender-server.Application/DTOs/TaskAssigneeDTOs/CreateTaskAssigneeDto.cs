namespace slender_server.Application.DTOs.TaskAssigneeDTOs;
using Domain.Entities;

public sealed record CreateTaskAssigneeDto
{
    public required string TaskId { get; init; }
    public required string UserId { get; init; }
    public string? AssignedById { get; init; }
}

public static class CreateTaskAssigneeDtoExtensions
{
    public static TaskAssignee ToEntity(this CreateTaskAssigneeDto dto)
    {
        return new TaskAssignee
        {
            TaskId = dto.TaskId,
            UserId = dto.UserId,
            AssignedById = dto.AssignedById,
            AssignedAtUtc = DateTime.UtcNow
        };
    }
}
