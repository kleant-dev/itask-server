using slender_server.Application.DTOs.NotificationDTOs;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;

namespace slender_server.Application.Interfaces.Services;

public interface INotificationService
{
    Task<Result<NotificationDto>> CreateNotificationAsync(
        CreateNotificationDto dto,
        CancellationToken ct = default);

    Task<Result<PagedResponse<NotificationDto>>> GetUserNotificationsAsync(
        string userId,
        PaginationParams pagination,
        SortParams sort,
        CancellationToken ct = default);

    Task<Result<NotificationDto>> UpdateNotificationAsync(
        string notificationId,
        string userId,
        UpdateNotificationDto dto,
        CancellationToken ct = default);
}

