using slender_server.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace slender_server.Domain.Interfaces;

/// <summary>
/// ChannelMember has composite key (ChannelId, UserId). No single-id GetByIdAsync.
/// </summary>
public interface IChannelMemberRepository
{
    Task<ChannelMember?> GetByChannelAndUserAsync(string channelId, string userId, CancellationToken ct = default);
    Task<List<ChannelMember>> GetByChannelIdAsync(string channelId, CancellationToken ct = default);
    Task<bool> IsMemberAsync(string channelId, string userId, CancellationToken ct = default);
    Task AddAsync(ChannelMember entity, CancellationToken ct = default);
    Task UpdateAsync(ChannelMember entity, CancellationToken ct = default);
    Task RemoveAsync(ChannelMember entity, CancellationToken ct = default);
}
