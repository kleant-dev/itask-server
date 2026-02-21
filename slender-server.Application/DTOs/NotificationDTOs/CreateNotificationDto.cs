using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.NotificationDTOs;

public sealed record CreateNotificationDto
{
    public required string RecipientId { get; init; }
    public string? ActorId { get; init; }
    public required NotificationType Type { get; init; }
    public string? WorkspaceId { get; init; }
    public string? ProjectId { get; init; }
    public string? TaskId { get; init; }
    public required string Title { get; init; }
    public string? Body { get; init; }
    public string? EntityName { get; init; }
}

public static class CreateNotificationDtoExtensions
{
    public static Notification ToEntity(this CreateNotificationDto dto)
    {
        return new Notification
        {
            Id = Notification.NewId(),
            RecipientId = dto.RecipientId,
            ActorId = dto.ActorId,
            Type = dto.Type,
            WorkspaceId = dto.WorkspaceId,
            ProjectId = dto.ProjectId,
            TaskId = dto.TaskId,
            Title = dto.Title,
            Body = dto.Body,
            EntityName = dto.EntityName,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
