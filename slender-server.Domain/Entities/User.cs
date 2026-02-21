namespace slender_server.Domain.Entities;

public sealed class User
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public string? AvatarUrl { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarColor { get; set; } 
    public DateTime? LastActiveAtUtc { get; set; }

    /// <summary>
    /// We'll use this to store the IdentityId from the Identity Provider.
    /// This could be any identity provider like Azure AD, Cognito, Keycloak, Auth0, etc.
    /// </summary>
    public string IdentityId { get; set; } = string.Empty;
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }  
    
    public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = [];

    public static string NewId() => $"u-{Guid.CreateVersion7()}";
}