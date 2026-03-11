using slender_server.Application.DTOs.MessageDTOs;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;

namespace slender_server.Application.Interfaces.Services;

public interface IMessageService
{
    Task<Result<MessageDto>> CreateAsync(string userId, CreateMessageDto dto, CancellationToken ct = default);
    Task<Result<PagedResponse<MessageDto>>> GetByChannelAsync(string channelId, string userId, PaginationParams pagination, CancellationToken ct = default);
    Task<Result<MessageDto>> GetByIdAsync(string messageId, string userId, CancellationToken ct = default);
    Task<Result<MessageDto>> UpdateAsync(string messageId, string userId, UpdateMessageDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string messageId, string userId, CancellationToken ct = default);
}
