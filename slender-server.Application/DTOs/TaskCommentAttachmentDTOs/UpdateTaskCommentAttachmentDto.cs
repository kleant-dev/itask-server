using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.TaskCommentAttachmentDTOs;

public sealed record UpdateTaskCommentAttachmentDto
{
    public string? FileName { get; init; }
    public string? FileUrl { get; init; }
    public long? FileSizeBytes { get; init; }
    public string? MimeType { get; init; }
}

public static class UpdateTaskCommentAttachmentDtoExtensions
{
    public static void ApplyTo(this UpdateTaskCommentAttachmentDto dto, TaskCommentAttachment entity)
    {
        if (dto.FileName is not null) entity.FileName = dto.FileName;
        if (dto.FileUrl is not null) entity.FileUrl = dto.FileUrl;
        if (dto.FileSizeBytes is not null) entity.FileSizeBytes = dto.FileSizeBytes.Value;
        if (dto.MimeType is not null) entity.MimeType = dto.MimeType;
    }
}
