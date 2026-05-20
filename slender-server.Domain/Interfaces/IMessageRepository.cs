using slender_server.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace slender_server.Domain.Interfaces;

public interface IMessageRepository : IRepository<Message>
{
    Task<Message?> GetByIdWithAuthorAsync(string messageId, CancellationToken ct = default);
    Task<List<Message>> GetByChannelIdAsync(string channelId, int skip, int take, CancellationToken ct = default);
    Task<int> GetCountByChannelIdAsync(string channelId, CancellationToken ct = default);
    Task<List<Message>> GetUnreadByChannelAsync(string channelId, string userId, CancellationToken ct = default);
    Task MarkAsReadAsync(IEnumerable<Message> messages, CancellationToken ct = default);
}
