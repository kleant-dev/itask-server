namespace slender_server.Domain.Entities;

public sealed class TaskAssignee
{
    public required string TaskId { get; set; }
    public required string UserId { get; set; }
    public string? AssignedById { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    
    public Task Task { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? AssignedBy { get; set; }
    
    // Composite PK: (TaskId, UserId)
}
