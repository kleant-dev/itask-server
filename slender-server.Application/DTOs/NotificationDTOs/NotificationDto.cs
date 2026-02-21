using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.NotificationDTOs;

public sealed record NotificationDto
{
    public required string Id { get; init; }
    public required string RecipientId { get; init; }
    public string? ActorId { get; init; }
    public required NotificationType Type { get; init; }
    public string? WorkspaceId { get; init; }
    public string? ProjectId { get; init; }
    public string? TaskId { get; init; }
    public required string Title { get; init; }
    public string? Body { get; init; }
    public string? EntityName { get; init; }
    public DateTime? ReadAtUtc { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public static class NotificationDtoExtensions
{
    public static NotificationDto ToDto(this Notification entity)
    {
        return new NotificationDto
        {
            Id = entity.Id,
            RecipientId = entity.RecipientId,
            ActorId = entity.ActorId,
            Type = entity.Type,
            WorkspaceId = entity.WorkspaceId,
            ProjectId = entity.ProjectId,
            TaskId = entity.TaskId,
            Title = entity.Title,
            Body = entity.Body,
            EntityName = entity.EntityName,
            ReadAtUtc = entity.ReadAtUtc,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
