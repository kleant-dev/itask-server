using slender_server.Domain.Entities;
using TaskStatus = slender_server.Domain.Entities.TaskStatus;

namespace slender_server.Application.DTOs.TaskDTOs;

public sealed record UpdateTaskDto
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public TaskStatus? Status { get; init; }
    public TaskPriority? Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int? DurationMinutes { get; init; }
    public double? SortOrder { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}

public static class UpdateTaskDtoExtensions
{
    public static void ApplyTo(this UpdateTaskDto dto, Domain.Entities.Task entity)
    {
        if (dto.Title is not null) entity.Title = dto.Title;
        if (dto.Description is not null) entity.Description = dto.Description;
        if (dto.Status is not null) entity.Status = dto.Status.Value;
        if (dto.Priority is not null) entity.Priority = dto.Priority.Value;
        if (dto.DueDate is not null) entity.DueDate = dto.DueDate;
        if (dto.ScheduledAt is not null) entity.ScheduledAt = dto.ScheduledAt;
        if (dto.DurationMinutes is not null) entity.DurationMinutes = dto.DurationMinutes;
        if (dto.SortOrder is not null) entity.SortOrder = dto.SortOrder.Value;
        if (dto.CompletedAtUtc is not null) entity.CompletedAtUtc = dto.CompletedAtUtc;
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }
}
