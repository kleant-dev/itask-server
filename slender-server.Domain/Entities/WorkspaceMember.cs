namespace slender_server.Domain.Entities;

public sealed class WorkspaceMember
{
    public required string WorkspaceId { get; set; }
    public required string UserId { get; set; }
    public WorkspaceRole Role { get; set; }
    public string? InvitedByUserId { get; set; }
    public DateTime JoinedAtUtc { get; set; }
    
    public Workspace Workspace { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? InvitedByUser { get; set; }
}

public enum WorkspaceRole
{
    Owner,
    Admin,
    Member,
    Guest
}