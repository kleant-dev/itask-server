using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IChannelRepository : IRepository<Channel>
{
    Task<Channel?> GetByIdWithMembersAsync(string channelId, CancellationToken ct = default);
    Task<List<Channel>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken ct = default);
    Task<Channel?> GetByParticipantHashAsync(string workspaceId, string participantHash, CancellationToken ct = default);
    Task<bool> ExistsAsync(string channelId, CancellationToken ct = default);
}
