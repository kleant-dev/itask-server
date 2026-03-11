using slender_server.Application.DTOs.NotificationDTOs;
using slender_server.Application.Models.Common;

namespace slender_server.Application.Interfaces.Services;

public interface INotificationService
{
    Task<Result<NotificationDto>> CreateNotificationAsync(
        CreateNotificationDto dto,
        CancellationToken ct = default);

    Task<Result<IReadOnlyCollection<NotificationDto>>> GetUserNotificationsAsync(
        string userId,
        CancellationToken ct = default);

    Task<Result<NotificationDto>> UpdateNotificationAsync(
        string notificationId,
        string userId,
        UpdateNotificationDto dto,
        CancellationToken ct = default);
}

