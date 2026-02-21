namespace slender_server.Application.DTOs.TaskAttachmentDTOs;
using Domain.Entities;

public sealed record TaskAttachmentDto
{
    public required string Id { get; init; }
    public required string TaskId { get; init; }
    public required string UploadedById { get; init; }
    public required string FileName { get; init; }
    public required string FileUrl { get; init; }
    public long FileSizeBytes { get; init; }
    public required string MimeType { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public static class TaskAttachmentDtoExtensions
{
    public static TaskAttachmentDto ToDto(this TaskAttachment entity)
    {
        return new TaskAttachmentDto
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            UploadedById = entity.UploadedById,
            FileName = entity.FileName,
            FileUrl = entity.FileUrl,
            FileSizeBytes = entity.FileSizeBytes,
            MimeType = entity.MimeType,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
