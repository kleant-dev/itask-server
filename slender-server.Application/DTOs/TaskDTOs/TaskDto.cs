using slender_server.Domain.Entities;
using TaskStatus = slender_server.Domain.Entities.TaskStatus;

namespace slender_server.Application.DTOs.TaskDTOs;

public sealed record TaskDto
{
    public required string Id { get; init; }
    public required string ProjectId { get; init; }
    public required string WorkspaceId { get; init; }
    public string? CreatedById { get; init; }
    public string? ParentTaskId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required TaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int? DurationMinutes { get; init; }
    public double SortOrder { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}

public static class TaskDtoExtensions
{
    public static TaskDto ToDto(this Domain.Entities.Task entity)
    {
        return new TaskDto
        {
            Id = entity.Id,
            ProjectId = entity.ProjectId,
            WorkspaceId = entity.WorkspaceId,
            CreatedById = entity.CreatedById,
            ParentTaskId = entity.ParentTaskId,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status,
            Priority = entity.Priority,
            DueDate = entity.DueDate,
            ScheduledAt = entity.ScheduledAt,
            DurationMinutes = entity.DurationMinutes,
            SortOrder = entity.SortOrder,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc
        };
    }
}
