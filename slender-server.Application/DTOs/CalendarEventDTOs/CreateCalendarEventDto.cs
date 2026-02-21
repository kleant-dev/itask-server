using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.CalendarEventDTOs;

public sealed record CreateCalendarEventDto
{
    public required string WorkspaceId { get; init; }
    public required string CreatedById { get; init; }
    public string? TaskId { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public string? ScheduleType { get; init; }
    public required DateTime StartsAtUtc { get; init; }
    public required DateTime EndsAtUtc { get; init; }
    public bool IsAllDay { get; init; }
}

public static class CreateCalendarEventDtoExtensions
{
    public static CalendarEvent ToEntity(this CreateCalendarEventDto dto)
    {
        return new CalendarEvent
        {
            Id = CalendarEvent.NewId(),
            WorkspaceId = dto.WorkspaceId,
            CreatedById = dto.CreatedById,
            TaskId = dto.TaskId,
            Title = dto.Title,
            Description = dto.Description,
            ScheduleType = dto.ScheduleType,
            StartsAtUtc = dto.StartsAtUtc,
            EndsAtUtc = dto.EndsAtUtc,
            IsAllDay = dto.IsAllDay,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
