using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;

namespace slender_server.Application.Services;

/// <summary>
/// Handles message persistence. Real-time delivery is the responsibility of ChatHub.
/// Follows the same pattern as existing services: repo → UoW → Result.
/// </summary>
public sealed class MessageService(
    IMessageRepository messageRepository,
    IChannelMemberRepository channelMemberRepository,
    IRepository<Message> messageRepo,
    IUnitOfWork unitOfWork) : IMessageService
{
    public async Task<Result<MessageDto>> CreateAsync(string userId, CreateMessageDto dto, CancellationToken ct)
    {
        // Verify the channel exists and the user is a member
        var isMember = await channelMemberRepository
            .IsMemberAsync(dto.ChannelId, userId, ct);
        if (!isMember)
            return Result<MessageDto>.Failure("You are not a member of this channel.",ErrorType.Forbidden);

        var message = dto.ToEntity();
        // Override AuthorId with verified userId (dto.AuthorId is set from hub but we re-verify)
        message.AuthorId = userId;

        await messageRepo.AddAsync(message, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<MessageDto>.Success(message.ToDto());
    }

    public async Task<Result<PagedResponse<MessageDto>>> GetByChannelAsync(
        string channelId,
        string userId,
        PaginationParams pagination,
        CancellationToken ct)
    {
        // Membership check
        var isMember = await channelMemberRepository
            .IsMemberAsync(channelId, userId, ct);
        if (!isMember)
            return Result<PagedResponse<MessageDto>>.Failure("Access denied.",ErrorType.Forbidden);

        var items = await messageRepository
            .GetByChannelIdAsync(channelId, (pagination.PageNumber - 1) * pagination.PageSize, pagination.PageSize, ct);

        var total = await messageRepository.GetCountByChannelIdAsync(channelId,ct);
        var response = PagedResponse<MessageDto>.Create(
            items.Select(m => m.ToDto()).ToList(),
            pagination.PageNumber,
            pagination.PageSize,
            total);

        return Result<PagedResponse<MessageDto>>.Success(response);
    }
    
    public async Task<Result> MarkAsReadAsync(string channelId, string userId, CancellationToken ct)
    {
        try
        {
            var isMember = await channelMemberRepository
                .IsMemberAsync(channelId, userId, ct);
            if (!isMember)
                return Result.Failure("Access denied.", ErrorType.Forbidden);

            var unreadMessages = await messageRepository
                .GetUnreadByChannelAsync(channelId, userId, ct);

            if (unreadMessages.Any())
            {
                await messageRepository.MarkAsReadAsync(unreadMessages, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure("Failed to mark messages as read", ErrorType.NotFound);
        }
    }

    public async Task<Result<MessageDto>> UpdateAsync(string messageId, string userId, UpdateMessageDto dto, CancellationToken ct)
    {
        
        var message = await messageRepository
            .GetByIdAsync(messageId, ct);

        if (message is null)
            return Result<MessageDto>.Failure("Message not found.",ErrorType.NotFound);

        if (message.AuthorId != userId)
            return Result<MessageDto>.Failure("You can only edit your own messages.",ErrorType.Forbidden);

        dto.ApplyTo(message);

        await messageRepo.UpdateAsync(message, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<MessageDto>.Success(message.ToDto());
    }

    public async Task<Result<string>> DeleteAsync(string messageId, string userId, CancellationToken ct)
    {
        var message = await messageRepository
            .GetByIdAsync(messageId, ct);
        if (message is null)
            return Result<string>.Failure("Message not found.",ErrorType.NotFound);

        if (message.AuthorId != userId)
            return Result<string>.Failure("You can only delete your own messages.",ErrorType.Forbidden);

        var channelId = message.ChannelId;

        await messageRepo.DeleteAsync(message, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return Result<string>.Success(channelId);
    }
}