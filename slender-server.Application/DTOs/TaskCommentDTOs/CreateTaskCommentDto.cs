namespace slender_server.Application.DTOs.TaskCommentDTOs;
using Domain.Entities;

public sealed record CreateTaskCommentDto
{
    public required string TaskId { get; init; }
    public string? AuthorId { get; init; }
    public string? ParentCommentId { get; init; }
    public required string Body { get; init; }
}

public static class CreateTaskCommentDtoExtensions
{
    public static TaskComment ToEntity(this CreateTaskCommentDto dto)
    {
        return new TaskComment
        {
            Id = TaskComment.NewId(),
            TaskId = dto.TaskId,
            AuthorId = dto.AuthorId,
            ParentCommentId = dto.ParentCommentId,
            Body = dto.Body,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }
}
