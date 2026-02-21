using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.CalendarEventDTOs;

public sealed record CalendarEventDto
{
    public required string Id { get; init; }
    public required string WorkspaceId { get; init; }
    public required string CreatedById { get; init; }
    public string? TaskId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public string? ScheduleType { get; init; }
    public DateTime StartsAtUtc { get; init; }
    public DateTime EndsAtUtc { get; init; }
    public bool IsAllDay { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public static class CalendarEventDtoExtensions
{
    public static CalendarEventDto ToDto(this CalendarEvent entity)
    {
        return new CalendarEventDto
        {
            Id = entity.Id,
            WorkspaceId = entity.WorkspaceId,
            CreatedById = entity.CreatedById,
            TaskId = entity.TaskId,
            Title = entity.Title,
            Description = entity.Description,
            ScheduleType = entity.ScheduleType,
            StartsAtUtc = entity.StartsAtUtc,
            EndsAtUtc = entity.EndsAtUtc,
            IsAllDay = entity.IsAllDay,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
