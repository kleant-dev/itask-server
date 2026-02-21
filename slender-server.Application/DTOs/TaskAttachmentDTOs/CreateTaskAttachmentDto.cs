namespace slender_server.Application.DTOs.TaskAttachmentDTOs;
using Domain.Entities;

public sealed record CreateTaskAttachmentDto
{
    public required string TaskId { get; init; }
    public required string UploadedById { get; init; }
    public required string FileName { get; init; }
    public required string FileUrl { get; init; }
    public long FileSizeBytes { get; init; }
    public required string MimeType { get; init; }
}

public static class CreateTaskAttachmentDtoExtensions
{
    public static TaskAttachment ToEntity(this CreateTaskAttachmentDto dto)
    {
        return new TaskAttachment
        {
            Id = TaskAttachment.NewId(),
            TaskId = dto.TaskId,
            UploadedById = dto.UploadedById,
            FileName = dto.FileName,
            FileUrl = dto.FileUrl,
            FileSizeBytes = dto.FileSizeBytes,
            MimeType = dto.MimeType,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
