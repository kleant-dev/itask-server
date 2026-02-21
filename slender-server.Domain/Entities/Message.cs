namespace slender_server.Domain.Entities;

public sealed class Message
{
    public required string Id { get; set; }
    public required string ChannelId { get; set; }
    public required string AuthorId { get; set; }
    public string? ReplyToId { get; set; }              // threading
    
    public required string Body { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; } 
    
    public Channel Channel { get; set; } = null!;
    public User Author { get; set; } = null!;
    public Message? ReplyToMessage { get; set; }
    public ICollection<Message> Replies { get; set; } = [];        // soft delete
    
    public static string NewId() => $"m-{Guid.CreateVersion7()}";
}