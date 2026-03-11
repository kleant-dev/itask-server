using slender_server.Domain.Entities;

namespace slender_server.Domain.Interfaces;

public interface IMessageRepository : IRepository<Message>
{
    Task<Message?> GetByIdWithAuthorAsync(string messageId, CancellationToken ct = default);
    Task<List<Message>> GetByChannelIdAsync(string channelId, int skip, int take, CancellationToken ct = default);
    Task<int> GetCountByChannelIdAsync(string channelId, CancellationToken ct = default);
}
