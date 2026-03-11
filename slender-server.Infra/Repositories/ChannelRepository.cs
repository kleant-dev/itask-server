using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class ChannelRepository(ApplicationDbContext context) : Repository<Channel>(context), IChannelRepository
{
    public async Task<Channel?> GetByIdWithMembersAsync(string channelId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.Members)
            .ThenInclude(m => m.User)
            .Include(c => c.CreatedBy)
            .FirstOrDefaultAsync(c => c.Id == channelId, ct);
    }

    public async Task<List<Channel>> GetByWorkspaceIdAsync(string workspaceId, CancellationToken ct = default)
    {
        return await DbSet
            .Include(c => c.CreatedBy)
            .Where(c => c.WorkspaceId == workspaceId)
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<Channel?> GetByParticipantHashAsync(string workspaceId, string participantHash, CancellationToken ct = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.WorkspaceId == workspaceId && c.ParticipantHash == participantHash, ct);
    }

    public async Task<bool> ExistsAsync(string channelId, CancellationToken ct = default)
    {
        return await DbSet.AnyAsync(c => c.Id == channelId, ct);
    }
}
