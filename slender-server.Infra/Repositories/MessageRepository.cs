using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class MessageRepository(ApplicationDbContext context) : Repository<Message>(context), IMessageRepository
{
    public async Task<Message?> GetByIdWithAuthorAsync(string messageId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(m => m.Author)
            .Include(m => m.Channel)
            .Include(m => m.ReplyToMessage)
            .FirstOrDefaultAsync(m => m.Id == messageId, ct);
    }

    public async Task<List<Message>> GetByChannelIdAsync(string channelId, int skip, int take, CancellationToken ct = default)
    {
        return await DbSet
            .Include(m => m.Author)
            .Where(m => m.ChannelId == channelId && m.DeletedAtUtc == null)
            .OrderByDescending(m => m.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetCountByChannelIdAsync(string channelId, CancellationToken ct = default)
    {
        return await DbSet
            .CountAsync(m => m.ChannelId == channelId && m.DeletedAtUtc == null, ct);
    }
}
