namespace slender_server.Domain.Entities;

public sealed class Workspace
{
    public required string Id { get; set; }
    public required string OwnerId { get; set; }
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? LogoUrl { get; set; }
    public string? Description { get; set; }
    
    public ICollection<WorkspaceMember> Members { get; set; } = [];
    public ICollection<WorkspaceInvite> Invites { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
    public User Owner { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public static string NewId() => $"w-{Guid.CreateVersion7()}";
}