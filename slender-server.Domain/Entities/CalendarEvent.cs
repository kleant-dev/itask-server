namespace slender_server.Domain.Entities;

public sealed class CalendarEvent
{
    public required string Id { get; set; }
    public required string WorkspaceId { get; set; }
    public required string CreatedById { get; set; }
    public string? TaskId { get; set; }                 // optional link to task
    
    public required string Title { get; set; }
    public string? Description { get; set; }            // for AddScheduleModal
    public string? ScheduleType { get; set; }           // Meeting/Task/Review/Event/Reminder
    
    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }
    public bool IsAllDay { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    
    public static string NewId() => $"ce-{Guid.CreateVersion7()}";
}