namespace slender_server.Domain.Entities;

public sealed class Notification
{
    public required string Id { get; set; }
    public required string RecipientId { get; set; }
    public string? ActorId { get; set; }                // who triggered it (null = system)
    
    public required NotificationType Type { get; set; }
    
    // Polymorphic links
    public string? WorkspaceId { get; set; }
    public string? ProjectId { get; set; }
    public string? TaskId { get; set; }
    
    public required string Title { get; set; }
    public string? Body { get; set; }
    public string? EntityName { get; set; }             // e.g. task title for rich display
    
    public DateTime? ReadAtUtc { get; set; }            // null = unread
    public DateTime CreatedAtUtc { get; set; }
    
    public static string NewId() => $"n-{Guid.CreateVersion7()}";
}

public enum NotificationType
{
    TaskAssigned,
    Mentioned,
    Commented,
    Deadline,
    InviteAccepted,
    ProjectUpdate
}