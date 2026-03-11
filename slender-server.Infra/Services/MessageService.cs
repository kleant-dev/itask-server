using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Interfaces.Services;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Domain.Models;

namespace slender_server.Infra.Services;

public sealed class MessageService(
    IMessageRepository messageRepository,
    IChannelMemberRepository channelMemberRepository,
    IUnitOfWork unitOfWork,
    IPaginationService paginationService)
    : IMessageService
{
    public async Task<Result<MessageDto>> CreateAsync(string userId, CreateMessageDto dto, CancellationToken ct = default)
    {
        var isMember = await channelMemberRepository.IsMemberAsync(dto.ChannelId, userId, ct);
        if (!isMember)
            return Result<MessageDto>.Failure("You do not have access to this channel");

        var entity = (dto with { AuthorId = userId }).ToEntity();
        await messageRepository.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<MessageDto>.Success(entity.ToDto());
    }

    public async Task<Result<PagedResponse<MessageDto>>> GetByChannelAsync(string channelId, string userId, PaginationParams pagination, CancellationToken ct = default)
    {
        var isMember = await channelMemberRepository.IsMemberAsync(channelId, userId, ct);
        if (!isMember)
            return Result<PagedResponse<MessageDto>>.Failure("You do not have access to this channel");

        var total = await messageRepository.GetCountByChannelIdAsync(channelId, ct);
        var skip = (pagination.PageNumber - 1) * pagination.PageSize;
        var items = await messageRepository.GetByChannelIdAsync(channelId, skip, pagination.PageSize, ct);
        var paged = new PagedResult<Message>(items.ToList(), total, pagination.PageNumber, pagination.PageSize);
        var response = paginationService.MapToPagedResponse(paged, m => m.ToDto());
        return Result<PagedResponse<MessageDto>>.Success(response);
    }

    public async Task<Result<MessageDto>> GetByIdAsync(string messageId, string userId, CancellationToken ct = default)
    {
        var message = await messageRepository.GetByIdWithAuthorAsync(messageId, ct);
        if (message is null)
            return Result<MessageDto>.Failure("Message not found");

        var isMember = await channelMemberRepository.IsMemberAsync(message.ChannelId, userId, ct);
        if (!isMember)
            return Result<MessageDto>.Failure("You do not have access to this message");

        return Result<MessageDto>.Success(message.ToDto());
    }

    public async Task<Result<MessageDto>> UpdateAsync(string messageId, string userId, UpdateMessageDto dto, CancellationToken ct = default)
    {
        var message = await messageRepository.GetByIdAsync(messageId, ct);
        if (message is null)
            return Result<MessageDto>.Failure("Message not found");

        var isMember = await channelMemberRepository.IsMemberAsync(message.ChannelId, userId, ct);
        if (!isMember)
            return Result<MessageDto>.Failure("You do not have access to this message");
        if (message.AuthorId != userId)
            return Result<MessageDto>.Failure("Only the author can edit this message");

        dto.ApplyTo(message);
        await messageRepository.UpdateAsync(message, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result<MessageDto>.Success(message.ToDto());
    }

    public async Task<Result> DeleteAsync(string messageId, string userId, CancellationToken ct = default)
    {
        var message = await messageRepository.GetByIdAsync(messageId, ct);
        if (message is null)
            return Result.Failure("Message not found");

        var isMember = await channelMemberRepository.IsMemberAsync(message.ChannelId, userId, ct);
        if (!isMember)
            return Result.Failure("You do not have access to this message");
        if (message.AuthorId != userId)
            return Result.Failure("Only the author can delete this message");

        message.DeletedAtUtc = DateTime.UtcNow;
        await messageRepository.UpdateAsync(message, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
