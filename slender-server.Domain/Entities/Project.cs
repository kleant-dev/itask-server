namespace slender_server.Domain.Entities;

public sealed class Project
{
    public required string Id { get; set; }
    public required string WorkspaceId { get; set; }
    public required string OwnerId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required ProjectStatus Status { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? TargetDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? ArchivedAtUtc { get; set; }
    
    public Workspace Workspace { get; set; } = null!;
    public User Owner { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = [];
    
    public static string NewId() => $"p-{Guid.CreateVersion7()}";
}

public enum ProjectStatus
{
    Active,
    OnHold,
    Completed,
    Archived
}