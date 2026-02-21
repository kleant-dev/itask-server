namespace slender_server.Domain.Entities;

public sealed class TaskComment
{
    public required string Id { get; set; }
    public required string TaskId { get; set; }
    public string? AuthorId { get; set; }
    public string? ParentCommentId { get; set; }        // threading (1 level)
    
    public required string Body { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; }         // soft delete

    public Task Task { get; set; } = null!;
    public User? Author { get; set; }
    public TaskComment? ParentComment { get; set; }
    public ICollection<TaskComment> Replies { get; set; } = [];
    public ICollection<TaskCommentAttachment> Attachments { get; set; } = [];
    
    public static string NewId() => $"tc-{Guid.CreateVersion7()}";
}