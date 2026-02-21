namespace slender_server.Domain.Entities;

public sealed class ChannelMember
{
    public required string ChannelId { get; set; }
    public string? UserId { get; set; }
    
    public DateTime? LastReadAtUtc { get; set; }        // drives unread badge
    public DateTime JoinedAtUtc { get; set; }

    public Channel Channel { get; set; } = null!;
    public User? User { get; set; } = null!;
    
    // Composite PK: (ChannelId, UserId)
}
