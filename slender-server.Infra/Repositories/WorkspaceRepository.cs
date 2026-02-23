using Microsoft.EntityFrameworkCore;
using slender_server.Domain.Entities;
using slender_server.Domain.Interfaces;
using slender_server.Infra.Database;

namespace slender_server.Infra.Repositories;

public sealed class WorkspaceRepository : Repository<Workspace>, IWorkspaceRepository
{
    public WorkspaceRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Workspace?> GetByIdWithMembersAsync(string workspaceId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(w => w.Members)
            .ThenInclude(m => m.User)
            .Include(w => w.Owner)
            .FirstOrDefaultAsync(w => w.Id == workspaceId, ct);
    }

    public async Task<List<Workspace>> GetUserWorkspacesAsync(string userId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(w => w.Members.Any(m => m.UserId == userId))
            .OrderByDescending(w => w.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<bool> IsUserMemberAsync(string workspaceId, string userId, CancellationToken ct = default)
    {
        return await _dbSet
            .AnyAsync(w => w.Id == workspaceId && w.Members.Any(m => m.UserId == userId), ct);
    }

    public async Task<Workspace?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(w => w.Slug == slug, ct);
    }
}