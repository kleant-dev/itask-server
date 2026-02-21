namespace slender_server.Domain.Entities;

public sealed class TaskCommentAttachment
{
    public required string Id { get; set; }
    public required string CommentId { get; set; }
    public required string UploadedById { get; set; }
    public required string FileName { get; set; }
    public required string FileUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public required string MimeType { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    
    public TaskComment TaskComment { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
    
    public static string NewId() => $"tca-{Guid.CreateVersion7()}";
}