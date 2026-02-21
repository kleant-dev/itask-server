namespace slender_server.Domain.Entities;

public sealed class ActivityLog
{
    public required string Id { get; set; }
    public required string WorkspaceId { get; set; }
    public required string ActorId { get; set; }
    
    public required string Action { get; set; }         // task_created, status_changed, etc
    public required string EntityType { get; set; }     // task, project, workspace, comment, member
    public required string EntityId { get; set; }       // polymorphic
    
    public string? OldValue { get; set; }               // JSON string
    public string? NewValue { get; set; }               // JSON string
    
    public DateTime CreatedAtUtc { get; set; }
    
    public static string NewId() => $"al-{Guid.CreateVersion7()}";
}