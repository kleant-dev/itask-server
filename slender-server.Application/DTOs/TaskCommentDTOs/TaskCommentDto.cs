namespace slender_server.Application.DTOs.TaskCommentDTOs;
using Domain.Entities;

public sealed record TaskCommentDto
{
    public required string Id { get; init; }
    public required string TaskId { get; init; }
    public string? AuthorId { get; init; }
    public string? ParentCommentId { get; init; }
    public required string Body { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime UpdatedAtUtc { get; init; }
}

public static class TaskCommentDtoExtensions
{
    public static TaskCommentDto ToDto(this TaskComment entity)
    {
        return new TaskCommentDto
        {
            Id = entity.Id,
            TaskId = entity.TaskId,
            AuthorId = entity.AuthorId,
            ParentCommentId = entity.ParentCommentId,
            Body = entity.Body,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        };
    }
}
