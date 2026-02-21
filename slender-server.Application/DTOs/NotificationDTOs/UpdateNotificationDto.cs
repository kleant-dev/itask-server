using slender_server.Domain.Entities;

namespace slender_server.Application.DTOs.NotificationDTOs;

public sealed record UpdateNotificationDto
{
    public DateTime? ReadAtUtc { get; init; }
}

public static class UpdateNotificationDtoExtensions
{
    public static void ApplyTo(this UpdateNotificationDto dto, Notification entity)
    {
        if (dto.ReadAtUtc is not null) entity.ReadAtUtc = dto.ReadAtUtc;
    }
}
