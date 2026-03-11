using slender_server.Application.DTOs.ChannelDTOs;
using slender_server.Application.DTOs.ChannelMemberDTOs;
using slender_server.Application.Models.Common;
using slender_server.Application.Models.Pagination;

namespace slender_server.Application.Interfaces.Services;

public interface IChannelService
{
    Task<Result<ChannelDto>> CreateAsync(string userId, CreateChannelDto dto, CancellationToken ct = default);
    Task<Result<ChannelDto>> GetOrCreateDmAsync(string userId, string workspaceId, string otherUserId, CancellationToken ct = default);
    Task<Result<PagedResponse<ChannelDto>>> GetByWorkspaceAsync(string workspaceId, string userId, PaginationParams pagination, CancellationToken ct = default);
    Task<Result<ChannelDto>> GetByIdAsync(string channelId, string userId, CancellationToken ct = default);
    Task<Result<ChannelDto>> UpdateAsync(string channelId, string userId, UpdateChannelDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string channelId, string userId, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ChannelMemberDto>>> GetMembersAsync(string channelId, string userId, CancellationToken ct = default);
    Task<Result<ChannelMemberDto>> JoinAsync(string channelId, string userId, CancellationToken ct = default);
    Task<Result> LeaveAsync(string channelId, string userId, CancellationToken ct = default);
}
