using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ActivityLog;

public sealed record CreateActivityLogDto
{
    public required string WorkspaceId { get; init; }
    public required string ActorId { get; init; }
    public required string Action { get; init; }
    public required string EntityType { get; init; }
    public required string EntityId { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}

public static class CreateActivityLogDtoExtensions
{
    public static Domain.Entities.ActivityLog ToEntity(this CreateActivityLogDto dto)
    {
        return new Domain.Entities.ActivityLog()
        {
            Id = Domain.Entities.ActivityLog.NewId(),
            WorkspaceId = dto.WorkspaceId,
            ActorId = dto.ActorId,
            Action = dto.Action,
            EntityType = dto.EntityType,
            EntityId = dto.EntityId,
            OldValue = dto.OldValue,
            NewValue = dto.NewValue,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
