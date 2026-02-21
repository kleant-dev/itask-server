using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.ActivityLog;

/// <summary>
/// Activity logs are typically append-only. Update provided for consistency.
/// </summary>
public sealed record UpdateActivityLogDto
{
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
}

public static class UpdateActivityLogDtoExtensions
{
    public static void ApplyTo(this UpdateActivityLogDto dto, Domain.Entities.ActivityLog entity)
    {
        if (dto.OldValue is not null) entity.OldValue = dto.OldValue;
        if (dto.NewValue is not null) entity.NewValue = dto.NewValue;
    }
}
