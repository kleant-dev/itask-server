namespace slender_server.Domain.Entities;

public sealed class TaskAttachment
{
    public required string Id { get; set; }
    public required string TaskId { get; set; }
    public required string UploadedById { get; set; }
    
    public required string FileName { get; set; }
    public required string FileUrl { get; set; }
    public long FileSizeBytes { get; set; }
    public required string MimeType { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    
    public Task Task { get; set; } = null!;
    public User UploadedBy { get; set; } = null!;
    
    public static string NewId() => $"ta-{Guid.CreateVersion7()}";
}