using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ActivityLog;

public sealed record ActivityLogDto
{
    public required string Id { get; init; }
    public required string WorkspaceId { get; init; }
    public required string ActorId { get; init; }
    public required string Action { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}

public static class ActivityLogDtoExtensions
{
    public static ActivityLogDto ToDto(this Domain.Entities.ActivityLog entity)
    {
        return new ActivityLogDto
        {
            Id = entity.Id,
            WorkspaceId = entity.WorkspaceId,
            ActorId = entity.ActorId,
            Action = entity.Action,
            EntityType = entity.EntityType,
            EntityId = entity.EntityId,
            OldValue = entity.OldValue,
            NewValue = entity.NewValue,
            CreatedAtUtc = entity.CreatedAtUtc
        };
    }
}
