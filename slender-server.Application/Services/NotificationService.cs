using slender_server.Application.DTOs.NotificationDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Application.Models.Sorting;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;

namespace slender_server.Application.Services;

public sealed class NotificationService(
    IRepository<Notification> notificationRepository,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService)
    : INotificationService
{
    public async Task<Result<NotificationDto>> CreateNotificationAsync(
        CreateNotificationDto dto,
        CancellationToken ct = default)
    {
        var entity = dto.ToEntity();
        await notificationRepository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<NotificationDto>.Success(entity.ToDto());
    }

    public async Task<Result<PagedResponse<NotificationDto>>> GetUserNotificationsAsync(
        string userId,
        PaginationParams pagination,
        SortParams sort,
        CancellationToken ct = default)
    {
        var pagedResult = await notificationRepository.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            filter: n => n.RecipientId == userId,
            orderBy: q => q.OrderByDescending(n => n.CreatedAtUtc),
            ct: ct);

        var pagedResponse = paginationService.MapToPagedResponse(
            pagedResult, 
            n => n.ToDto());

        return Result<PagedResponse<NotificationDto>>.Success(pagedResponse);
    }

    public async Task<Result<NotificationDto>> UpdateNotificationAsync(
        string notificationId,
        string userId,
        UpdateNotificationDto dto,
        CancellationToken ct = default)
    {
        var notification = await notificationRepository.GetByIdAsync(notificationId, ct);
        if (notification is null)
            return Result<NotificationDto>.Failure("Notification not found",ErrorType.NotFound);

        if (notification.RecipientId != userId)
            return Result<NotificationDto>.Failure("You cannot update this notification",ErrorType.Forbidden);

        dto.ApplyTo(notification);
        await notificationRepository.UpdateAsync(notification, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<NotificationDto>.Success(notification.ToDto());
    }
}

