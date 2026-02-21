using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.CalendarEventDTOs;

public sealed record UpdateCalendarEventDto
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? ScheduleType { get; init; }
    public string? TaskId { get; init; }
    public DateTime? StartsAtUtc { get; init; }
    public DateTime? EndsAtUtc { get; init; }
    public bool? IsAllDay { get; init; }
}

public static class UpdateCalendarEventDtoExtensions
{
    public static void ApplyTo(this UpdateCalendarEventDto dto, CalendarEvent entity)
    {
        if (dto.Title is not null) entity.Title = dto.Title;
        if (dto.Description is not null) entity.Description = dto.Description;
        if (dto.ScheduleType is not null) entity.ScheduleType = dto.ScheduleType;
        if (dto.TaskId is not null) entity.TaskId = dto.TaskId;
        if (dto.StartsAtUtc is not null) entity.StartsAtUtc = dto.StartsAtUtc.Value;
        if (dto.EndsAtUtc is not null) entity.EndsAtUtc = dto.EndsAtUtc.Value;
        if (dto.IsAllDay is not null) entity.IsAllDay = dto.IsAllDay.Value;
    }
}
