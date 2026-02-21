using System.Security.Cryptography;
using System.Text;

namespace slender_server.Domain.Entities;

public sealed class Channel
{
    public required string Id { get; set; }
    public required string WorkspaceId { get; set; }
    public string? CreatedById { get; set; }
    public required ChannelType Type { get; set; }
    
    public string? Name { get; set; }
    public string? ProjectId { get; set; }
    
    // FIX #3: Use hash instead of sorted IDs (ChatGPT recommendation)
    // Fixed length, order-independent, index-friendly
    public string? ParticipantHash { get; set; }  // SHA256 hash of participant IDs
    
    public DateTime CreatedAtUtc { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public User? CreatedBy { get; set; } = null!;
    public Project? Project { get; set; }
    public ICollection<ChannelMember> Members { get; set; } = [];
    public ICollection<Message> Messages { get; set; } = [];
    
    public static string NewId() => $"ch-{Guid.CreateVersion7()}";
    
    /// <summary>
    /// Create a DM channel between users.
    /// Hash is order-independent: Hash(alice,bob) == Hash(bob,alice)
    /// </summary>
    public static Channel CreateDirectMessage(
        string workspaceId,
        string createdById,
        params string[] participantIds)
    {
        if (participantIds.Length < 2)
            throw new ArgumentException("DM requires at least 2 participants");
        
        return new Channel
        {
            Id = NewId(),
            WorkspaceId = workspaceId,
            CreatedById = createdById,
            Type = ChannelType.DirectMessage,
            Name = null,
            ProjectId = null,
            ParticipantHash = ComputeParticipantHash(participantIds),
            CreatedAtUtc = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Compute SHA256 hash of participant IDs.
    /// Order-independent: sorts IDs before hashing.
    /// </summary>
    private static string ComputeParticipantHash(params string[] participantIds)
    {
        // Sort to ensure order-independence
        var sorted = participantIds.OrderBy(id => id).ToArray();
        var combined = string.Join("|", sorted);
        var bytes = Encoding.UTF8.GetBytes(combined);
        var hash = SHA256.HashData(bytes);

        // Return as hex string (64 chars)
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Find existing DM by participant IDs (order-independent)
    /// </summary>
    public static string GetParticipantHash(params string[] participantIds)
    {
        return ComputeParticipantHash(participantIds);
    }
}

public enum ChannelType
{
    Public,
    Private,
    DirectMessage
}