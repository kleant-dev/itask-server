namespace slender_server.Domain.Entities;

public sealed class WorkspaceInvite
{
    public required string Id { get; set; }
    public required string WorkspaceId { get; set; }
    public required string Email { get; set; }
    public required string InvitedByUserId { get; set; }
    public required string Token { get; set; }
    public required WorkspaceRole Role { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? AcceptedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    
    public Workspace Workspace { get; set; } = null!;
    public User InvitedByUser { get; set; } = null!;
    public static string NewId() => $"wi-{Guid.CreateVersion7()}";
}