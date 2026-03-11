using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;
using Task = System.Threading.Tasks.Task;

namespace slender_server.Infra.Repositories;

public sealed class ChannelMemberRepository(ApplicationDbContext context) : IChannelMemberRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly DbSet<ChannelMember> _set = context.ChannelMembers;

    public async Task<ChannelMember?> GetByChannelAndUserAsync(string channelId, string userId, CancellationToken ct = default)
    {
        return await _set
            .Include(cm => cm.Channel)
            .Include(cm => cm.User)
            .FirstOrDefaultAsync(cm => cm.ChannelId == channelId && cm.UserId == userId, ct);
    }

    public async Task<List<ChannelMember>> GetByChannelIdAsync(string channelId, CancellationToken ct = default)
    {
        return await _set
            .Include(cm => cm.User)
            .Where(cm => cm.ChannelId == channelId)
            .OrderBy(cm => cm.JoinedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<bool> IsMemberAsync(string channelId, string userId, CancellationToken ct = default)
    {
        return await _set.AnyAsync(cm => cm.ChannelId == channelId && cm.UserId == userId, ct);
    }

    public async Task AddAsync(ChannelMember entity, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
    }

    public Task UpdateAsync(ChannelMember entity, CancellationToken ct = default)
    {
        _set.Update(entity);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(ChannelMember entity, CancellationToken ct = default)
    {
        _set.Remove(entity);
        return Task.CompletedTask;
    }
}
