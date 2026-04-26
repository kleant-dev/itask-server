using Microsoft.EntityFrameworkCore;
using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Services;

/// <summary>
/// Handles message persistence. Real-time delivery is the responsibility of ChatHub.
/// Follows the same pattern as existing services: repo → UoW → Result.
/// </summary>
public sealed class MessageService(
    ApplicationDbContext db,
    IRepository<Message> messageRepo,
    IUnitOfWork unitOfWork) : IMessageService
{
    public async Task<Result<MessageDto>> CreateAsync(string userId, CreateMessageDto dto, CancellationToken ct)
    {
        // Verify the channel exists and the user is a member
        var channel = await db.Channels
            .Include(c => c.Members)
            .FirstOrDefaultAsync(c => c.Id == dto.ChannelId, ct);

        if (channel is null)
            return Result<MessageDto>.Failure("Channel not found.",ErrorType.NotFound);

        var isMember = channel.Members.Any(m => m.UserId == userId);
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
        var isMember = await db.ChannelMembers
            .AnyAsync(m => m.ChannelId == channelId && m.UserId == userId, ct);

        if (!isMember)
            return Result<PagedResponse<MessageDto>>.Failure("Access denied.",ErrorType.Forbidden);

        var query = db.Messages
            .Where(m => m.ChannelId == channelId)
            .OrderBy(m => m.CreatedAtUtc);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync(ct);

        var response = PagedResponse<MessageDto>.Create(
            items.Select(m => m.ToDto()).ToList(),
            pagination.PageNumber,
            pagination.PageSize,
            total);

        return Result<PagedResponse<MessageDto>>.Success(response);
    }

    public async Task<Result<MessageDto>> UpdateAsync(string messageId, string userId, UpdateMessageDto dto, CancellationToken ct)
    {
        var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == messageId, ct);

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
        var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == messageId, ct);

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