namespace slender_server.Domain.Entities;

public sealed class ProjectMember
{
    public required string ProjectId { get; set; }
    public required string UserId { get; set; }
    public required ProjectMemberRole Role { get; set; }
    public string? AddedByUserId { get; set; }
    public DateTime JoinedAtUtc { get; set; }
    
    public Project Project { get; set; } = null!;
    public User User { get; set; } = null!;
    public User? AddedByUser { get; set; }
}

public enum ProjectMemberRole
{
    Lead,
    Member,
    Viewer
}