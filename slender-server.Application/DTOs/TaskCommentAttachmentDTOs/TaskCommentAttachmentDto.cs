using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.TaskCommentAttachmentDTOs;

public sealed record TaskCommentAttachmentDto
{
    public required string Id { get; init; }
    public required string CommentId { get; init; }
    public required string UploadedById { get; init; }
    public required string FileName { get; init; }
    public required string FileUrl { get; init; }
    public long FileSizeBytes { get; init; }
    public required string MimeType { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public static class TaskCommentAttachmentDtoExtensions
{
    public static TaskCommentAttachmentDto ToDto(this TaskCommentAttachment entity)
    {
        return new TaskCommentAttachmentDto
        {
            Id = entity.Id,
            CommentId = entity.CommentId,
            UploadedById = entity.UploadedById,
            FileName = entity.FileName,
            FileUrl = entity.FileUrl,
            FileSizeBytes = entity.FileSizeBytes,
            MimeType = entity.MimeType,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
