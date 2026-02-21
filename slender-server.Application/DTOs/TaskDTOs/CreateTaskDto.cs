using slender_server.Domain.Entities;
using Task = slender_server.Domain.Entities.Task;
using TaskStatus = slender_server.Domain.Entities.TaskStatus;

namespace slender_server.Application.DTOs.TaskDTOs;

public sealed record CreateTaskDto
{
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
}

public static class CreateTaskDtoExtensions
{
    public static Task ToEntity(this CreateTaskDto dto)
    {
        return new Task
        {
            Id = Task.NewId(),
            ProjectId = dto.ProjectId,
            WorkspaceId = dto.WorkspaceId,
            CreatedById = dto.CreatedById,
            ParentTaskId = dto.ParentTaskId,
            Title = dto.Title,
            Description = dto.Description,
            Status = dto.Status,
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            ScheduledAt = dto.ScheduledAt,
            DurationMinutes = dto.DurationMinutes,
            SortOrder = dto.SortOrder,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
