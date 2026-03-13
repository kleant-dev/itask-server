using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;

namespace slender_server.Application.Interfaces.Services;

public interface IMessageService
{
    /// <summary>Persist a new message and return the saved DTO.</summary>
    Task<Result<MessageDto>> CreateAsync(string userId, CreateMessageDto dto, CancellationToken ct);

    /// <summary>Return paginated messages for a channel (oldest-first).</summary>
    Task<Result<PagedResponse<MessageDto>>> GetByChannelAsync(
        string channelId,
        string userId,
        PaginationParams pagination,
        CancellationToken ct);

    /// <summary>Edit the body of an existing message. Only the author may edit.</summary>
    Task<Result<MessageDto>> UpdateAsync(string messageId, string userId, UpdateMessageDto dto, CancellationToken ct);

    /// <summary>
    /// Delete a message. Only the author may delete.
    /// Returns the channelId so callers can broadcast to the correct group.
    /// </summary>
    Task<Result<string>> DeleteAsync(string messageId, string userId, CancellationToken ct);
}