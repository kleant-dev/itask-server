using slender_server.Application.DTOs.NotificationDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;

namespace slender_server.Infra.Services;

public sealed class NotificationService(
    IRepository<Notification> notificationRepository,
    IUnitOfWork unitOfWork)
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

    public async Task<Result<IReadOnlyCollection<NotificationDto>>> GetUserNotificationsAsync(
        string userId,
        CancellationToken ct = default)
    {
        var all = await notificationRepository.GetAllAsync(predicate:(n)=>n.RecipientId == userId,ct:ct);
        var userNotifications = all
            .OrderByDescending(n => n.CreatedAtUtc)
            .Select(n => n.ToDto())
            .ToArray();

        return Result<IReadOnlyCollection<NotificationDto>>.Success(userNotifications);
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

