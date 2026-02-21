using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.TaskCommentAttachmentDTOs;

public sealed record CreateTaskCommentAttachmentDto
{
    public required string CommentId { get; init; }
    public required string UploadedById { get; init; }
    public required string FileName { get; init; }
    public required string FileUrl { get; init; }
    public long FileSizeBytes { get; init; }
    public required string MimeType { get; init; }
}

public static class CreateTaskCommentAttachmentDtoExtensions
{
    public static TaskCommentAttachment ToEntity(this CreateTaskCommentAttachmentDto dto)
    {
        return new TaskCommentAttachment
        {
            Id = TaskCommentAttachment.NewId(),
            CommentId = dto.CommentId,
            UploadedById = dto.UploadedById,
            FileName = dto.FileName,
            FileUrl = dto.FileUrl,
            FileSizeBytes = dto.FileSizeBytes,
            MimeType = dto.MimeType,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
